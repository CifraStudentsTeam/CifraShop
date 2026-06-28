---
feature: cookie-consent-banner
status: delivered
specs: []
plans:
  - docs/compose/plans/2026-06-28-cookie-consent-banner.md
branch: NewCIfraShop
commits: 50d46e0..812ff10
---

# Cookie Consent Banner — Final Report

## What Was Built

Added a simple cookie consent banner to the CifraShop Blazor WebAssembly client. The banner appears at the bottom of every page, informing users about cookie usage and allowing them to accept. The consent choice persists in localStorage, so users only see the banner once until they clear their browser data.

## Architecture

The implementation consists of three files:

1. **`CifraShop.Client/Components/CookieConsent.razor`** - Reusable Blazor component that checks localStorage for existing consent and displays the banner if not present. Uses JavaScript Interop to read/write localStorage.

2. **`CifraShop.Client/App.razor`** - Modified to include the `<CookieConsent />` component after the Router, ensuring it appears on all pages.

3. **`CifraShop.Client/wwwroot/css/cookie-consent.css`** - Custom CSS for responsive styling (mobile-friendly centered layout, full-width button on small screens).

### Design Decisions

- **localStorage over cookies**: Chosen because the consent state is a simple boolean flag that doesn't need to be sent to the server. localStorage is simpler and doesn't interfere with the existing JWT authentication flow.

- **Component in App.razor**: Placed outside the Router to ensure it renders on all pages, including 404 pages. This is the simplest way to achieve global visibility without modifying every page.

- **Bootstrap classes**: Used existing Bootstrap utility classes (`position-fixed`, `bottom-0`, `bg-dark`, etc.) to maintain visual consistency with the rest of the application.

## Usage

The banner automatically appears for new visitors. Once the user clicks "Принять" (Accept), the consent state is stored in localStorage under the key `cookieConsent` with value `accepted`. The banner will not reappear unless the user clears their browser data.

No configuration or API changes were required. The feature is purely client-side.

## Verification

- Build verification: `dotnet build CifraShop.Client/CifraShop.Client.csproj` succeeded with 0 errors, 0 warnings
- Manual testing required: Run `dotnet run --project CifraShop.Client` and verify in browser at `http://localhost:5001`

## Journey Log

- [lesson] Blazor WebAssembly components outside the Router still render on all pages, making App.razor a good location for global UI elements like banners.

## Source Materials

| File | Role | Notes |
|------|------|-------|
| `docs/compose/plans/2026-06-28-cookie-consent-banner.md` | Implementation plan | Complete |