import { useTranslations } from "next-intl";
import { Link } from "@/i18n/navigation";

const ADDRESS = "mun. Chisinau, str. St. cel Mare, 12";
const EMAIL = "info@pometco.com";
const PHONE_DISPLAY = "+373 (79) 222-444";
const PHONE_TEL = "+37379222444";

const SOCIAL_LINKS = [
  { icon: "chat", label: "Viber" },
  { icon: "public", label: "Website" },
  { icon: "share", label: "Telegram" },
  { icon: "photo_camera", label: "Instagram" },
];

const PAYMENT_ICONS = ["payments", "credit_card", "account_balance"];

export function Footer() {
  const t = useTranslations("Footer");

  return (
    <footer className="w-full bg-on-tertiary-fixed text-tertiary-fixed">
      <div className="mx-auto grid max-w-[1280px] grid-cols-1 gap-gutter px-4 py-16 sm:px-12 md:grid-cols-4">
        <div>
          <div className="mb-6 flex items-center gap-2">
            <span
              className="material-symbols-outlined text-3xl text-white"
              style={{ fontVariationSettings: "'FILL' 1" }}
            >
              call_to_action
            </span>
            <span className="font-headline text-xl font-bold text-white">
              Pometco
            </span>
          </div>
          <p className="mb-6 text-sm leading-relaxed text-tertiary-fixed-dim">
            {t("tagline")}
          </p>
          {/* Real social accounts don't exist yet (see ROADMAP.md, "Реальные
              соцсети") — placeholder links, not a broken-feature regression. */}
          <div className="flex gap-4">
            {SOCIAL_LINKS.map((social) => (
              <a
                key={social.icon}
                href="#"
                aria-label={social.label}
                className="flex h-10 w-10 items-center justify-center rounded-full bg-white/10 transition-colors hover:bg-primary"
              >
                <span className="material-symbols-outlined text-[20px]">
                  {social.icon}
                </span>
              </a>
            ))}
          </div>
        </div>

        <div className="flex flex-col gap-3">
          <h4 className="mb-1 font-bold text-white">{t("company")}</h4>
          <Link
            href="/"
            className="text-sm text-tertiary-fixed-dim hover:text-white"
          >
            {t("home")}
          </Link>
          <Link
            href="/about"
            className="text-sm text-tertiary-fixed-dim hover:text-white"
          >
            {t("aboutUs")}
          </Link>
          <Link
            href="/routes"
            className="text-sm text-tertiary-fixed-dim hover:text-white"
          >
            {t("routes")}
          </Link>
        </div>

        <div className="flex flex-col gap-3">
          <h4 className="mb-1 font-bold text-white">{t("contactInfo")}</h4>
          <a
            href={`https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(ADDRESS)}`}
            target="_blank"
            rel="noopener noreferrer"
            className="flex gap-3 text-sm text-tertiary-fixed-dim hover:text-white"
          >
            <span className="material-symbols-outlined text-lg">
              location_on
            </span>
            <span>{ADDRESS}</span>
          </a>
          <a
            href={`mailto:${EMAIL}`}
            className="flex gap-3 text-sm text-tertiary-fixed-dim hover:text-white"
          >
            <span className="material-symbols-outlined text-lg">mail</span>
            <span>{EMAIL}</span>
          </a>
          <a
            href={`tel:${PHONE_TEL}`}
            className="flex gap-3 text-sm text-tertiary-fixed-dim hover:text-white"
          >
            <span className="material-symbols-outlined text-lg">call</span>
            <span>{PHONE_DISPLAY}</span>
          </a>
        </div>

        <div className="flex flex-col gap-6">
          <div className="flex flex-col gap-3">
            <h4 className="mb-1 font-bold text-white">{t("securePayments")}</h4>
            <div className="flex flex-wrap gap-4 opacity-60 grayscale">
              {PAYMENT_ICONS.map((icon) => (
                <span key={icon} className="material-symbols-outlined text-4xl">
                  {icon}
                </span>
              ))}
            </div>
          </div>
          <div className="flex flex-col gap-2">
            <h4 className="mb-1 font-bold text-white">{t("legal")}</h4>
            <Link
              href="#"
              className="text-sm text-tertiary-fixed-dim hover:text-white"
            >
              {t("privacyPolicy")}
            </Link>
            <Link
              href="#"
              className="text-sm text-tertiary-fixed-dim hover:text-white"
            >
              {t("termsOfService")}
            </Link>
          </div>
        </div>
      </div>

      <div className="border-t border-white/10 py-6 text-center text-xs text-tertiary-fixed-dim">
        {t("copyright", { year: new Date().getFullYear() })}
      </div>
    </footer>
  );
}
