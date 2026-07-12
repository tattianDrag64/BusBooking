import { useTranslations } from "next-intl";
import { PageHero } from "@/components/ui/PageHero";
import { MissionSection } from "@/components/about/MissionSection";
import { NewsSection } from "@/components/ui/NewsSection";
import { ContactSection } from "@/components/home/ContactSection";

export const metadata = {
  title: "About Us — Pometco",
};

export default function AboutPage() {
  const t = useTranslations("About");

  return (
    <>
      <PageHero title={t("heroTitle")} />
      <MissionSection />
      <NewsSection />
      <ContactSection />
    </>
  );
}
