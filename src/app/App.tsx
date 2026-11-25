import { BrowserRouter } from "react-router-dom";
import { useBackdropClose } from "@/lib/useBackdropClose";
import { AppRouter } from "./router/router";
export function App() {
  useBackdropClose();
  return (
    <BrowserRouter>
      <AppRouter />
    </BrowserRouter>
  );
}
