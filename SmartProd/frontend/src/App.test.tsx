import { render, screen } from "@testing-library/react"
import { beforeEach, describe, expect, it, vi } from "vitest"
import { useAuth } from "./contexts/AuthContext"
import App from "./App"

vi.mock("./contexts/AuthContext", () => ({
  useAuth: vi.fn(),
}))

vi.mock("./pages/Login", () => ({
  default: () => <div>Login screen</div>,
}))

vi.mock("./components/ui/sonner", () => ({
  Toaster: () => null,
}))

describe("protected routes", () => {
  beforeEach(() => {
    window.history.replaceState({}, "", "/bom-list")
    vi.mocked(useAuth).mockReturnValue({
      user: null,
      loading: false,
      login: vi.fn(),
      register: vi.fn(),
      logout: vi.fn(),
    })
  })

  it("redirects unauthenticated visitors from a protected route to login", async () => {
    render(<App />)

    expect(await screen.findByText("Login screen")).toBeInTheDocument()
  })
})
