import { branding } from "./branding.content";

type BrandLogoProps = {
  variant?: "header" | "panel" | "compact" | "footer";
  showSymbol?: boolean;
  tagline?: boolean;
};

export function BrandLogo({
  variant = "header",
  showSymbol = true,
  tagline = false,
}: BrandLogoProps) {
  return (
    <span className={`capnexa-brand capnexa-brand--${variant}`}>
      {showSymbol && (
        <img
          className="capnexa-brand__symbol"
          src={branding.symbol}
          width={505}
          height={494}

          alt=""
        />
      )}

      <span className="capnexa-brand__text">
        <span dir="ltr" lang="en" className="capnexa-brand__wordmark">
          {branding.name.slice(0, branding.accentIndex)}
          <span className="capnexa-brand__accent">{branding.name[branding.accentIndex]}</span>
          {branding.name.slice(branding.accentIndex + 1)}
        </span>
        {tagline && <span className="capnexa-brand__tagline">{branding.tagline}</span>}
      </span>
    </span>
  );
}
