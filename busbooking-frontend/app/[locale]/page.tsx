import { getRoutes } from "@/api/routes";
import { HeroSection } from "@/components/home/HeroSection";
import { SearchWidget } from "@/components/home/SearchWidget";
import { RouteExplorer } from "@/components/home/RouteExplorer";
import { WhyPometco } from "@/components/home/WhyPometco";
import { NewsSection } from "@/components/ui/NewsSection";
import { ContactSection } from "@/components/home/ContactSection";

export default async function Home() {
  const routes = await getRoutes();

  return (
    <>
      <HeroSection />

      <section className="relative z-10 mx-auto -mt-24 max-w-[1280px] px-4 sm:px-12">
        <SearchWidget routes={routes} />
      </section>

      <RouteExplorer routes={routes} />
      <WhyPometco />
      <NewsSection background="plain" />
      <ContactSection />
    </>
  );
}
