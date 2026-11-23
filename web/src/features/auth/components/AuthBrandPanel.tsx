import { StitchIcon } from "./StitchIcon";
import { BrandLogo } from "../../../components/branding/BrandLogo";
export function AuthBrandPanel() {
  const features = [
    {
      icon: "hub" as const,
      title: "Centralized Management",
      text: "Keep project work in one place.",
    },
    {
      icon: "fact_check" as const,
      title: "AI-Assisted Academic Integrity",
      text: "AI assistance for academic integrity workflows.",
    },
    {
      icon: "verified_user" as const,
      title: "Secure Collaboration",
      text: "Collaborate securely across the capstone journey.",
    },
  ];
  return (
    <aside className="brand-panel" aria-label="About CAPNEXA">
      <svg
        className="constellation"
        viewBox="0 0 720 860"
        fill="none"
        aria-hidden="true"
        focusable="false"
        style={{
          position: "absolute",
          inset: 0,
          width: "100%",
          height: "100%",
          opacity: 0.4,
          pointerEvents: "none",
        }}
      >
        <defs>
          <radialGradient id="capnexa-node-glow">
            <stop offset="0%" stopColor="#269f93" stopOpacity="0.8" />
            <stop offset="100%" stopColor="#269f93" stopOpacity="0" />
          </radialGradient>

          <linearGradient id="capnexa-edge-one" x1="0%" y1="0%" x2="100%" y2="100%">
            <stop offset="0%" stopColor="#316bf3" stopOpacity="0.6" />
            <stop offset="100%" stopColor="#269f93" stopOpacity="0.3" />
          </linearGradient>

          <linearGradient id="capnexa-edge-two" x1="100%" y1="0%" x2="0%" y2="100%">
            <stop offset="0%" stopColor="#808dc2" stopOpacity="0.4" />
            <stop offset="100%" stopColor="#316bf3" stopOpacity="0.1" />
          </linearGradient>
        </defs>

        <path
          d="M120 180 L260 290 L480 210 L610 340 L490 520 L260 480 Z"
          stroke="url(#capnexa-edge-one)"
          strokeWidth="1.5"
          strokeDasharray="3 3"
        />
        <path
          d="M260 290 L390 390 L490 520 L320 640 L160 560 L260 480"
          stroke="url(#capnexa-edge-two)"
          strokeWidth="1.5"
        />
        <path d="M480 210 L390 390 L210 230" stroke="url(#capnexa-edge-one)" strokeWidth="1.2" />
        <path
          d="M390 390 L610 590 L490 730 L320 640"
          stroke="url(#capnexa-edge-two)"
          strokeWidth="1"
          strokeDasharray="4 2"
        />

        <circle cx="390" cy="390" r="160" stroke="#316bf3" strokeOpacity="0.15" strokeWidth="1" />
        <circle
          cx="390"
          cy="390"
          r="240"
          stroke="#269f93"
          strokeOpacity="0.1"
          strokeWidth="1"
          strokeDasharray="6 6"
        />

        <circle cx="120" cy="180" r="4" fill="#6bd8cb" />
        <circle cx="260" cy="290" r="6" fill="#89f5e7" />
        <circle cx="480" cy="210" r="5" fill="#316bf3" />
        <circle cx="610" cy="340" r="7" fill="#6bd8cb" />

        <circle cx="390" cy="390" r="22" fill="url(#capnexa-node-glow)" />
        <circle cx="390" cy="390" r="9" fill="#89f5e7" />

        <circle cx="260" cy="480" r="5" fill="#316bf3" />
        <circle cx="490" cy="520" r="6" fill="#89f5e7" />
        <circle cx="160" cy="560" r="4" fill="#316bf3" />
        <circle cx="320" cy="640" r="6" fill="#6bd8cb" />
        <circle cx="610" cy="590" r="5" fill="#808dc2" />
        <circle cx="490" cy="730" r="4" fill="#316bf3" />
      </svg>
      <div className="brand-content">
        <BrandLogo variant="panel" tagline />
        <h1>Your Capstone Journey, Connected</h1>
        <p className="brand-intro">Manage capstone work from proposal to evaluation.</p>
        <div className="feature-list">
          {features.map(({ icon, title, text }) => (
            <div className="feature-card" key={title}>
              <span className="feature-icon">
                <StitchIcon name={icon} size={20} />
              </span>
              <div>
                <h2>{title}</h2>
                <p>{text}</p>
              </div>
            </div>
          ))}
        </div>
      </div>
    </aside>
  );
}
