import Image from "next/image";
import { useTranslations } from "next-intl";
import { Link } from "@/i18n/navigation";

export function MissionSection() {
  const t = useTranslations("About");

  return (
    <section className="mx-auto max-w-[1280px] px-4 py-24 sm:px-12">
      <div className="flex flex-col items-center gap-12 overflow-hidden rounded-3xl bg-gradient-to-br from-secondary to-primary-container p-8 shadow-2xl md:p-16 lg:flex-row">
        <div className="flex-1 text-white">
          <h2 className="font-headline text-4xl font-bold leading-tight">
            {t("missionTitle")}
          </h2>
          <p className="mt-6 max-w-xl leading-relaxed text-white/90">
            {t("missionP1")}
          </p>
          <p className="mt-4 max-w-xl leading-relaxed text-white/90">
            {t("missionP2")}
          </p>
          <div className="mt-8 flex items-center gap-3">
            <span
              className="material-symbols-outlined text-3xl"
              style={{ fontVariationSettings: "'FILL' 1" }}
            >
              verified
            </span>
            <span className="text-sm font-semibold">{t("licensed")}</span>
          </div>
          <Link
            href="/routes"
            className="mt-8 inline-block rounded-full border border-white px-6 py-3 text-sm font-semibold text-white transition-colors hover:bg-white hover:text-primary"
          >
            {t("reserveNow")}
          </Link>
        </div>
        <div className="w-full max-w-lg flex-1">
          <Image
            src="/images/bus1.png"
            alt="Pometco coach"
            width={1856}
            height={1195}
            className="h-auto w-full drop-shadow-2xl transition-transform duration-500 hover:scale-105"
            priority={false}
          />
        </div>
      </div>
    </section>
  );
}
