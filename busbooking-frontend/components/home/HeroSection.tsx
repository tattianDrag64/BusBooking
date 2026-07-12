import { useTranslations } from "next-intl";

export function HeroSection() {
  const t = useTranslations("Home");

  return (
    <section className="relative flex h-[560px] items-center overflow-hidden bg-gradient-to-br from-deep-navy via-primary to-primary-container">
      <div
        aria-hidden
        className="absolute inset-0 opacity-30"
        style={{
          backgroundImage:
            "radial-gradient(circle at 20% 20%, rgba(255,255,255,0.25), transparent 40%), radial-gradient(circle at 80% 60%, rgba(255,255,255,0.15), transparent 45%)",
        }}
      />
      <div className="relative z-10 mx-auto w-full max-w-[1280px] px-4 pb-28 sm:px-12">
        <div className="max-w-2xl">
          <h1 className="font-headline text-4xl font-bold leading-tight text-white sm:text-6xl">
            {t("heroTitle")}
          </h1>
          <p className="mt-6 max-w-lg text-lg text-white/90">
            {t("heroSubtitle")}
          </p>
        </div>
      </div>
    </section>
  );
}
