# Cookie Consent Banner Implementation Plan

> [!NOTE]
> This document may not reflect the current implementation.
> See the final report for up-to-date state:
> [Final Report](../reports/cookie-consent-banner.md)

> **For agentic workers:** REQUIRED SUB-SKILL: Use compose:subagent (recommended) or compose:execute to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a simple cookie consent banner at the bottom of the page that notifies users about cookie usage and allows them to accept.

**Architecture:** Create a reusable Blazor component `CookieConsent.razor` that displays a fixed banner at the bottom of the screen. The component stores the user's consent choice in localStorage to persist across sessions. Add the component to `App.razor` so it appears on all pages.

**Tech Stack:** Blazor WebAssembly, Bootstrap 5.3.8, JavaScript Interop (for localStorage)

## Global Constraints

- UI text must be in Russian
- Use existing Bootstrap classes for styling
- Component must be reusable across all pages
- Consent state must persist in localStorage
- Banner must be fixed at bottom of screen

---

### Task 1: Create CookieConsent Component

**Covers:** [S1]

**Files:**
- Create: `CifraShop.Client/Components/CookieConsent.razor`

**Interfaces:**
- Consumes: None (standalone component)
- Produces: `CookieConsent` component with `OnAccept` event

- [ ] **Step 1: Create the CookieConsent.razor component**

```razor
@inject IJSRuntime JS

@if (!_consentGiven)
{
    <div class="cookie-consent-banner position-fixed bottom-0 start-0 w-100 bg-dark text-light p-3 shadow-lg" style="z-index: 1050;">
        <div class="container d-flex flex-column flex-md-row justify-content-between align-items-center gap-3">
            <div class="d-flex align-items-center">
                <i class="bi bi-cookie me-2 fs-4"></i>
                <span>Мы используем cookies для улучшения работы сайта. Продолжая использовать сайт, вы соглашаетесь с использованием cookies.</span>
            </div>
            <button class="btn btn-primary btn-sm px-4" @onclick="AcceptCookies">
                <i class="bi bi-check-circle me-1"></i>Принять
            </button>
        </div>
    </div>
}

@code {
    private bool _consentGiven = true;

    protected override async Task OnInitializedAsync()
    {
        var consent = await JS.InvokeAsync<string>("localStorage.getItem", "cookieConsent");
        _consentGiven = consent == "accepted";
    }

    private async Task AcceptCookies()
    {
        await JS.InvokeVoidAsync("localStorage.setItem", "cookieConsent", "accepted");
        _consentGiven = true;
        StateHasChanged();
    }
}
```

- [ ] **Step 2: Verify component compiles**

Run: `dotnet build CifraShop.Client/CifraShop.Client.csproj`
Expected: Build succeeds

- [ ] **Step 3: Commit**

```bash
git add CifraShop.Client/Components/CookieConsent.razor
git commit -m "feat: add CookieConsent Blazor component"
```

### Task 2: Add Component to App.razor

**Covers:** [S1]

**Files:**
- Modify: `CifraShop.Client/App.razor:1-13`

**Interfaces:**
- Consumes: `CookieConsent` component from Task 1
- Produces: Updated App.razor with banner on all pages

- [ ] **Step 1: Add CookieConsent component to App.razor**

```razor
@using CifraShop.Client.Components

<Router AppAssembly="@typeof(App).Assembly">
    <Found Context="routeData">
        <RouteView RouteData="@routeData" />
    </Found>
    <NotFound>
        <PageTitle>Не найдено</PageTitle>
        <div class="container py-5 text-center">
            <h3 class="fw-bold">404</h3>
            <p class="text-secondary">Страница не найдена</p>
            <a href="/" class="btn btn-primary btn-sm">На главную</a>
        </div>
    </NotFound>
</Router>

<CookieConsent />
```

- [ ] **Step 2: Verify build**

Run: `dotnet build CifraShop.Client/CifraShop.Client.csproj`
Expected: Build succeeds

- [ ] **Step 3: Commit**

```bash
git add CifraShop.Client/App.razor
git commit -m "feat: integrate CookieConsent banner in App.razor"
```

### Task 3: Add CSS for Banner

**Covers:** [S1]

**Files:**
- Modify: `CifraShop.Client/wwwroot/index.html` or create `CifraShop.Client/wwwroot/css/cookie-consent.css`

**Interfaces:**
- Consumes: CookieConsent component from Task 1
- Produces: Styled banner with proper positioning

- [ ] **Step 1: Create CSS file for cookie consent styling**

Create `CifraShop.Client/wwwroot/css/cookie-consent.css`:

```css
.cookie-consent-banner {
    border-top: 1px solid rgba(255, 255, 255, 0.1);
    backdrop-filter: blur(10px);
}

@media (max-width: 768px) {
    .cookie-consent-banner {
        text-align: center;
    }
    
    .cookie-consent-banner .btn {
        width: 100%;
    }
}
```

- [ ] **Step 2: Add CSS reference to index.html**

Modify `CifraShop.Client/wwwroot/index.html` to add after line 10 (after bootstrap-icons CSS):
```html
<link rel="stylesheet" href="css/cookie-consent.css" />
```

- [ ] **Step 3: Verify build**

Run: `dotnet build CifraShop.Client/CifraShop.Client.csproj`
Expected: Build succeeds

- [ ] **Step 4: Commit**

```bash
git add CifraShop.Client/wwwroot/css/cookie-consent.css CifraShop.Client/wwwroot/index.html
git commit -m "style: add CSS for cookie consent banner"
```

### Task 4: Test Banner Functionality

**Covers:** [S1]

**Files:**
- None (testing only)

**Interfaces:**
- Consumes: All previous tasks
- Produces: Verified functionality

- [ ] **Step 1: Run the client application**

Run: `dotnet run --project CifraShop.Client`
Expected: Client starts on port 5001

- [ ] **Step 2: Test in browser**

1. Open browser to `http://localhost:5001`
2. Verify banner appears at bottom of page
3. Click "Принять" button
4. Refresh page - banner should not appear
5. Clear localStorage - banner should reappear

- [ ] **Step 3: Commit test results (optional)**

No commit needed - this is manual testing.

---

## Self-Review

**1. Spec coverage:** Task 1-4 cover the cookie consent banner feature completely.

**2. Placeholder scan:** No placeholders found - all steps have concrete code.

**3. Type consistency:** Component names and method signatures are consistent across tasks.