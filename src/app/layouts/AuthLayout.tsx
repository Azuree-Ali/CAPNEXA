import { useEffect, useRef, useState } from "react";
import { Outlet, useLocation } from "react-router-dom";
import { X } from "lucide-react";
import { BrandLogo } from "../../components/branding/BrandLogo";

export function AuthLayout() {
  const [info, setInfo] = useState<string | null>(null);
  const dialogRef = useRef<HTMLDialogElement>(null);
  const { pathname } = useLocation();

  useEffect(() => {
    const pageTitle =
      pathname === "/forgot-password"
        ? "Reset password"
        : pathname === "/login"
          ? "Sign in"
          : "Page not found";

    document.title = `${pageTitle} | CAPNEXA`;
  }, [pathname]);

  useEffect(() => {
    if (info && !dialogRef.current?.open) {
      dialogRef.current?.showModal();
    }
  }, [info]);

  function closeDialog() {
    dialogRef.current?.close();
  }

  function infoButton(label: string) {
    return (
      <button
        type="button"
        className="text-button"
        onClick={() => setInfo(label)}
      >
        {label}
      </button>
    );
  }

  return (
    <div className="auth-layout">
      <a className="skip-link" href="#main-content">
        Skip to content
      </a>

      <header className="site-header">
        <div className="header-inner">
          <div className="header-brand">
            <BrandLogo variant="header" />
          </div>

          <nav aria-label="Support links">
            {infoButton("Privacy")}
            {infoButton("Help")}
          </nav>
        </div>
      </header>

      <main id="main-content" className="auth-main">
        <Outlet context={{ openInfo: setInfo }} />
      </main>

      <footer className="site-footer">
        <div className="footer-inner">
          <div className="copyright-line">
            <span>©</span>
            <BrandLogo variant="footer" showSymbol={false} />
            <span>All rights reserved.</span>
          </div>

          <nav aria-label="Footer links">
            {infoButton("Privacy")}
            {infoButton("Help")}
          </nav>
        </div>
      </footer>

      <dialog
        ref={dialogRef}
        className="info-dialog"
        aria-labelledby="info-title"
        onClose={() => setInfo(null)}
      >
        <button
          type="button"
          className="dialog-close"
          aria-label="Close"
          onClick={closeDialog}
        >
          <X size={20} aria-hidden="true" />
        </button>

        <h2 id="info-title">{info}</h2>

        <p>
          The university will provide the official{" "}
          {info?.toLowerCase()} details before launch.
        </p>

        <button
          type="button"
          className="primary-button"
          onClick={closeDialog}
        >
          Close
        </button>
      </dialog>
    </div>
  );
}