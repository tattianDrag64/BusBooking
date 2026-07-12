// End-to-end smoke test for the BusBooking JSON API — no browser needed,
// this is a pure JSON API since Views/cookie-auth were removed (2026-07-10).
// Exercises: signin -> create trip (admin) -> list seats (autogenerate) ->
// book a seat -> conflict on double-booking -> my bookings -> users (admin)
// -> refresh -> revoke.
//
// Usage: node smoke-api.mjs
// Env:   BUSBOOKING_BASE_URL (default http://localhost:5100)
//        BUSBOOKING_ADMIN_EMAIL/PASSWORD (defaults: seeded admin xyz@mail.com / 1234)

const baseUrl = process.env.BUSBOOKING_BASE_URL || 'http://localhost:5100';
const adminEmail = process.env.BUSBOOKING_ADMIN_EMAIL || 'xyz@mail.com';
const adminPassword = process.env.BUSBOOKING_ADMIN_PASSWORD || '1234';

let failed = false;

function check(label, condition, extra = '') {
  const status = condition ? 'OK ' : 'FAIL';
  if (!condition) failed = true;
  console.log(`[${status}] ${label}${extra ? ' — ' + extra : ''}`);
}

async function api(method, path, { token, body } = {}) {
  const res = await fetch(new URL(path, baseUrl), {
    method,
    headers: {
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
    },
    body: body ? JSON.stringify(body) : undefined,
  });
  const text = await res.text();
  let json = null;
  try { json = text ? JSON.parse(text) : null; } catch { /* non-JSON response, e.g. health */ }
  return { status: res.status, json, text };
}

// 1. health
{
  const r = await api('GET', '/health');
  check('GET /health', r.status === 200, `status=${r.status} body=${r.text}`);
}

// 2. signin as seeded admin
const signin = await api('POST', '/api/auth/signin', { body: { email: adminEmail, password: adminPassword } });
check('POST /api/auth/signin (admin)', signin.status === 200 && signin.json?.accessToken, `status=${signin.status}`);
const adminToken = signin.json?.accessToken;
const refreshToken = signin.json?.refreshToken;

// 3. trips/search is public, no token
const search = await api('GET', '/api/trips/search?from=Sofia&to=Berlin&departureDate=2026-08-01&arrivalDate=2026-08-01&isReturnTrip=false');
check('GET /api/trips/search (no token)', search.status === 200, `status=${search.status}`);

// 4. api/users requires admin — reject with no token
const usersNoToken = await api('GET', '/api/users');
check('GET /api/users (no token) is 401', usersNoToken.status === 401, `status=${usersNoToken.status}`);

// 5. api/users with admin token
const users = await api('GET', '/api/users', { token: adminToken });
check('GET /api/users (admin token)', users.status === 200 && Array.isArray(users.json), `status=${users.status}`);

// 6. create a trip (need an existing bus/route id — reuse the first trip's bus if one exists,
//    otherwise this step is skipped; seat/booking checks below need at least one trip in the DB)
const existingTrips = await api('GET', '/api/trips', { token: adminToken });
let tripId = existingTrips.json?.[0]?.tripId;
if (!tripId) {
  console.log('[SKIP] No trip found and none created by this script (needs a seeded busId) — seat/booking checks skipped.');
} else {
  // 7. list seats for the trip (autogenerates on first call)
  const seats = await api('GET', `/api/seats/trip/${tripId}`, { token: adminToken });
  check('GET /api/seats/trip/{tripId}', seats.status === 200 && Array.isArray(seats.json) && seats.json.length > 0, `status=${seats.status} count=${seats.json?.length}`);

  const freeSeat = seats.json?.find(s => !s.isOccupied);
  if (freeSeat) {
    // 8. book it
    const book = await api('POST', '/api/seats/book', { token: adminToken, body: { tripId, seatId: freeSeat.id } });
    check('POST /api/seats/book', book.status === 200, `status=${book.status}`);

    // 9. booking the same seat again must conflict
    const bookAgain = await api('POST', '/api/seats/book', { token: adminToken, body: { tripId, seatId: freeSeat.id } });
    check('POST /api/seats/book (duplicate) is 409', bookAgain.status === 409, `status=${bookAgain.status}`);
  } else {
    console.log('[SKIP] No free seat available — booking checks skipped.');
  }

  // 10. my bookings
  const mine = await api('GET', '/api/bookings/my', { token: adminToken });
  check('GET /api/bookings/my', mine.status === 200 && Array.isArray(mine.json), `status=${mine.status} count=${mine.json?.length}`);
}

// 11. refresh
if (refreshToken) {
  const refreshed = await api('POST', '/api/auth/refresh', { body: { refreshToken } });
  check('POST /api/auth/refresh', refreshed.status === 200 && refreshed.json?.accessToken, `status=${refreshed.status}`);

  // 12. revoke (needs a valid access token to call, and the *new* refresh token from step 11)
  if (refreshed.json?.accessToken) {
    const revoke = await api('POST', '/api/auth/revoke', { token: refreshed.json.accessToken, body: { refreshToken: refreshed.json.refreshToken } });
    check('POST /api/auth/revoke', revoke.status === 204, `status=${revoke.status}`);
  }
}

console.log(failed ? '\nSMOKE TEST: FAILED (see FAIL lines above)' : '\nSMOKE TEST: all checks passed');
process.exit(failed ? 1 : 0);
