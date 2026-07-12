import { useTranslations } from "next-intl";

const FEATURES = [
  { icon: "chair", titleKey: "feature1Title", textKey: "feature1Text" },
  { icon: "sell", titleKey: "feature2Title", textKey: "feature2Text" },
  { icon: "verified_user", titleKey: "feature3Title", textKey: "feature3Text" },
] as const;

export function WhyPometco() {
  const t = useTranslations("Home");

  return (
    <section className="bg-surface-container-low py-24">
      <div className="mx-auto max-w-[1280px] px-4 sm:px-12">
        <h2 className="text-center font-headline text-3xl font-bold text-on-surface">
          {t("whyTitle")}
        </h2>
        <div className="mt-12 grid gap-6 sm:grid-cols-3">
          {FEATURES.map((feature) => (
            <div
              key={feature.titleKey}
              className="rounded-card bg-white p-8 shadow-sm transition-soft hover:shadow-lg"
            >
              <div className="mb-4 flex h-12 w-12 items-center justify-center rounded-full bg-surface-light">
                <span className="material-symbols-outlined text-primary">
                  {feature.icon}
                </span>
              </div>
              <h3 className="font-headline text-lg font-bold text-on-surface">
                {t(feature.titleKey)}
              </h3>
              <p className="mt-2 text-sm text-on-surface-variant">
                {t(feature.textKey)}
              </p>
            </div>
          ))}
        </div>
      </div>
    </section>
  );
}
