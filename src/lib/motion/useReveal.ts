import { useGSAP } from "@gsap/react";
import gsap from "gsap";
import { useRef } from "react";

const reduced = () => window.matchMedia("(prefers-reduced-motion: reduce)").matches;

/** Staggered entrance for every direct child matching `selector` inside the returned ref. */
export function useReveal<T extends HTMLElement>(selector = "[data-reveal]", delay = 0) {
  const scope = useRef<T>(null);
  useGSAP(
    () => {
      if (reduced()) return;
      gsap.from(selector, {
        autoAlpha: 0,
        y: 16,
        duration: 0.5,
        ease: "power2.out",
        stagger: 0.06,
        delay,
        clearProps: "transform,opacity,visibility",
      });
    },
    { scope },
  );
  return scope;
}
