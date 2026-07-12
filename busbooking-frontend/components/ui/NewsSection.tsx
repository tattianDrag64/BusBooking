import { NEWS_ITEMS } from "@/lib/news";

export function NewsSection({
  title = "Latest News",
  background = "low",
}: {
  title?: string;
  background?: "low" | "plain";
}) {
  return (
    <section
      className={`${background === "low" ? "bg-surface-container-low" : "bg-surface"} py-24`}
    >
      <div className="mx-auto max-w-[1280px] px-4 sm:px-12">
        <h2 className="text-center font-headline text-3xl font-bold text-on-surface">
          {title}
        </h2>
        <div className="mt-12 grid gap-6 sm:grid-cols-3">
          {NEWS_ITEMS.map((item) => (
            <article
              key={item.id}
              className="group rounded-3xl border border-outline-variant/30 bg-white p-8 shadow-sm transition-soft hover:shadow-md"
            >
              <div className="mb-4 flex h-12 w-12 items-center justify-center rounded-full bg-surface-light">
                <span className="material-symbols-outlined text-primary">
                  {item.icon}
                </span>
              </div>
              <p className="mb-3 flex items-center gap-2 text-sm text-text-muted">
                <span className="material-symbols-outlined text-sm">
                  calendar_today
                </span>
                {item.date}
              </p>
              <h3 className="font-headline text-lg font-bold text-on-surface transition-colors group-hover:text-primary">
                {item.title}
              </h3>
              <p className="mt-3 line-clamp-3 text-sm text-on-surface-variant">
                {item.excerpt}
              </p>
              <span className="mt-6 flex items-center gap-1 text-sm font-bold text-primary transition-all group-hover:gap-2">
                view more..
                <span className="material-symbols-outlined text-sm">
                  chevron_right
                </span>
              </span>
            </article>
          ))}
        </div>
      </div>
    </section>
  );
}
