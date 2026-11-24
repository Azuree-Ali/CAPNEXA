import { BrandLogo } from "../branding/BrandLogo";
import { branding } from "../branding/branding.content";

interface SiteFooterProps {
  onPrivacy: () => void;
  onHelp: () => void;
}
export function SiteFooter({ onPrivacy, onHelp }: SiteFooterProps) {
  return (
    <footer className="site-footer">
      <div className="footer-inner">
        <div className="copyright-line" dir="ltr" lang="en">
          {branding.copyright} <BrandLogo variant="footer" showSymbol={false} /> {branding.rights}
        </div>
        <nav aria-label={branding.footerNavigation}>
          <button type="button" className="text-button" onClick={onPrivacy}>
            {branding.privacy}
          </button>
          <button type="button" className="text-button" onClick={onHelp}>
            {branding.help}
          </button>
        </nav>
      </div>
    </footer>
  );
}
