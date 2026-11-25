import { useGSAP } from "@gsap/react";
import gsap from "gsap";
import { useRef } from "react";

/** Final value is always in the DOM (tests, screen readers, no-JS); the tween only animates visually. */
export function CountUp({ value, duration = 0.9 }: { value: number; duration?: number }) {
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
    { dependencies: [value], scope: ref },
  );
  return <span ref={ref}>{value}</span>;
}
