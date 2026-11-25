import { useGSAP } from "@gsap/react";
import gsap from "gsap";
import { useRef } from "react";

/** Keep the exact figure available to assistive technology during the visual tween. */
export function CountUp({ value, duration = 0.48 }: { value: number; duration?: number }) {
  const ref = useRef<HTMLSpanElement>(null);
  useGSAP(
    () => {
      const el = ref.current;
      if (!el || window.matchMedia("(prefers-reduced-motion: reduce)").matches) return;
      const state = { n: 0 };
      gsap.to(state, {
        n: value,
        duration,
        ease: "power2.out",
        onUpdate: () => {
          el.textContent = String(Math.round(state.n));
        },
        onComplete: () => {
          el.textContent = String(value);
        },
      });
    },
    { dependencies: [value, duration], revertOnUpdate: true, scope: ref },
  );
  return (
    <span className="dashboard-count">
      <span className="split-sr">{value}</span>
      <span ref={ref} aria-hidden="true">
        {value}
      </span>
    </span>
  );
}
