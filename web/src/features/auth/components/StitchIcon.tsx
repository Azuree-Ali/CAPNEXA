import {
  Workflow,
  Waypoints,
  ClipboardCheck,
  ShieldCheck,
  AtSign,
  LockKeyhole,
  Eye,
  EyeOff,
  ShieldUser,
  ArrowRight,
  type LucideIcon,
} from "lucide-react";

const icons = {
  account_tree: Workflow,
  hub: Waypoints,
  fact_check: ClipboardCheck,
  verified_user: ShieldCheck,
  alternate_email: AtSign,
  lock: LockKeyhole,
  visibility: Eye,
  visibility_off: EyeOff,
  admin_panel_settings: ShieldUser,
  arrow_forward: ArrowRight,
} satisfies Record<string, LucideIcon>;

type IconName = keyof typeof icons;

export function StitchIcon({ name, size = 20 }: { name: IconName; size?: number }) {
  const Icon = icons[name];

  return (
    <Icon
      size={size}
      strokeWidth={1.8}
      aria-hidden="true"
      focusable="false"
      style={{ flexShrink: 0, verticalAlign: "middle" }}
    />
  );
}
