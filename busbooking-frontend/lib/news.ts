import type { NewsItem } from "@/types/news";

export const NEWS_ITEMS: NewsItem[] = [
  {
    id: "snowfall-2024",
    date: "19-03-2024",
    title: "A powerful snowfall throughout the country",
    excerpt:
      "Our dispatch team adjusted schedules across every route to keep passengers safe during the season's heaviest snowfall.",
    icon: "ac_unit",
  },
  {
    id: "us-parcels-2024",
    date: "15-03-2024",
    title: "Expanding our US parcel delivery network",
    excerpt:
      "New routes and faster processing times for all transatlantic logistics operations starting this month.",
    icon: "local_shipping",
  },
  {
    id: "new-drivers-2024",
    date: "10-03-2024",
    title: "Meet our new experienced driver team",
    excerpt:
      "Safety is our priority. This month we've onboarded ten highly qualified professionals to our fleet.",
    icon: "verified_user",
  },
];
