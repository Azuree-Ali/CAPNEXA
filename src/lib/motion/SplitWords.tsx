import { useGSAP } from "@gsap/react";
import gsap from "gsap";
import { useRef } from "react";

/** Word-by-word headline reveal. Real text stays in a visually hidden span so the heading keeps its accessible name. */
export function SplitWords({ text }: { text: string }) {
  const ref = useRef<HTMLSpanElement>(null);
  useGSAP(
    () => {
      if (window.matchMedia("(prefers-reduced-motion: reduce)").matches) return;
      gsap.from(".split-word-inner", {
        yPercent: 110,
        duration: 0.7,
        ease: "power3.out",
        stagger: 0.07,
        delay: 0.15,
      });
    },
    { scope: ref },
  );
  return (
    <span ref={ref} className="split-words">
      <span className="split-sr">{text}</span>
      <span aria-hidden="true">
        {text.split(" ").map((word, i) => (
          <span key={i} className="split-word">
            <span className="split-word-inner">{word}</span>{" "}
          </span>
        ))}
      </span>
    </span>
  );
}
