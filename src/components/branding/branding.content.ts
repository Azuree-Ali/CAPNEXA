export interface BrandingContent {
  name: string;
  accentIndex: number;
  symbol: string;
  tagline: string;
  copyright: string;
  rights: string;
  privacy: string;
  help: string;
  footerNavigation: string;
  privacyNotice: string;
  close: string;
}
export const branding: BrandingContent = {
  name: "CAPNEXA",
  accentIndex: 5,
  symbol: "/branding/logo.png",
  tagline: "The Connected Capstone Journey",
  copyright: "©",
  rights: "All rights reserved.",
  privacy: "Privacy",
  help: "Help",
  footerNavigation: "Footer support",
  privacyNotice: "The university will provide its official privacy policy before launch.",
  close: "Close",
};
