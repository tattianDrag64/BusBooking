"use client";

import {
  createContext,
  useCallback,
  useEffect,
  useState,
  type ReactNode,
} from "react";
import { revoke, signIn as signInRequest, signUp as signUpRequest } from "@/api/auth";
import type { AuthUser, SignInPayload, SignUpPayload } from "@/types/auth";
import { decodeAccessToken } from "./decodeAccessToken";
import { clearTokens, getTokens, saveTokens } from "./tokenStorage";

interface AuthContextValue {
  user: AuthUser | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  signIn: (payload: SignInPayload) => Promise<void>;
  signUp: (payload: SignUpPayload) => Promise<void>;
  signOut: () => Promise<void>;
}

export const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const tokens = getTokens();
    if (tokens) {
      setUser(decodeAccessToken(tokens.accessToken));
    }
    setIsLoading(false);
  }, []);

  const signIn = useCallback(async (payload: SignInPayload) => {
    const tokens = await signInRequest(payload);
    saveTokens(tokens);
    setUser(decodeAccessToken(tokens.accessToken));
  }, []);

  const signUp = useCallback(async (payload: SignUpPayload) => {
    const tokens = await signUpRequest(payload);
    saveTokens(tokens);
    setUser(decodeAccessToken(tokens.accessToken));
  }, []);

  const signOut = useCallback(async () => {
    const tokens = getTokens();
    if (tokens) {
      try {
        await revoke(tokens.accessToken, tokens.refreshToken);
      } catch {
        // Server unreachable or token already invalid — still clear the
        // local session below so the user isn't stuck "logged in" client-side.
      }
    }
    clearTokens();
    setUser(null);
  }, []);

  return (
    <AuthContext.Provider
      value={{
        user,
        isAuthenticated: user !== null,
        isLoading,
        signIn,
        signUp,
        signOut,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}
