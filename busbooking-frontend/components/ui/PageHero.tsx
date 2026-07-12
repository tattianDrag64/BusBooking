export function PageHero({
  title,
  subtitle,
}: {
  title: string;
  subtitle?: string;
}) {
  return (
    <section className="relative flex h-72 items-center justify-center overflow-hidden bg-gradient-to-br from-deep-navy via-primary to-primary-container sm:h-80">
      <div
        aria-hidden
        className="absolute inset-0 opacity-30"
        style={{
          backgroundImage:
            "radial-gradient(circle at 20% 20%, rgba(255,255,255,0.25), transparent 40%), radial-gradient(circle at 80% 60%, rgba(255,255,255,0.15), transparent 45%)",
        }}
      />
      <div className="relative z-10 px-4 text-center">
        <h1 className="font-headline text-4xl font-bold uppercase tracking-tight text-white sm:text-6xl">
          {title}
        </h1>
        {subtitle && (
          <p className="mx-auto mt-4 max-w-lg text-white/90">{subtitle}</p>
        )}
        <div className="mx-auto mt-6 h-1.5 w-24 rounded-full bg-primary-fixed-dim" />
      </div>
    </section>
  );
}
