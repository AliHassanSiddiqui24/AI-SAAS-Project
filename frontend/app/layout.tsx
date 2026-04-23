import type { Metadata } from "next";
import { Geist, Geist_Mono } from "next/font/google";
import { Providers } from "./providers/Providers";
import { AuthProvider } from "./context/AuthContext";
import { HydrationFix } from "./components/HydrationFix";
import "./globals.css";

const geistSans = Geist({
  variable: "--font-geist-sans",
  subsets: ["latin"],
  display: "swap",
});

const geistMono = Geist_Mono({
  variable: "--font-geist-mono",
  subsets: ["latin"],
  display: "swap",
});

export const metadata: Metadata = {
  title: "AI CRM Pro",
  description: "AI-powered CRM solution for modern businesses",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html
      lang="en"
      className={`${geistSans.variable} ${geistMono.variable} h-full antialiased`}
    >
      <body className="min-h-full flex flex-col">
        <HydrationFix>
          <Providers>
            <AuthProvider>
              {children}
            </AuthProvider>
          </Providers>
        </HydrationFix>
      </body>
    </html>
  );
}
