type BrandLogoProps = {
  variant?: "header" | "panel" | "compact" | "footer";
  showSymbol?: boolean;
  tagline?: boolean;
};

const SYMBOL_SRC = "/branding/logo.png";

const WORDMARK_SRC = "/branding/text-logo.png";

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
          src={SYMBOL_SRC}
          width={505}
          height={494}
          style={{ objectFit: "contain" }}
          alt=""
        />
      )}

      <span className="capnexa-brand__text">
        <img
          className="capnexa-brand__wordmark"
          src={WORDMARK_SRC}
          width={1051}
          height={205}
          style={{ objectFit: "contain" }}
          alt="CAPNEXA"
        />

        {tagline && (
          <span className="capnexa-brand__tagline">
            The Connected Capstone Journey
          </span>
        )}
      </span>
    </span>
  );
}