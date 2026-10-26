import { render, screen, waitFor } from "@testing-library/react"
import userEvent from "@testing-library/user-event"
import { beforeEach, describe, expect, it, vi } from "vitest"
import { useApp } from "../contexts/AppContext"
import type { Product } from "../models"
import { BOMRegister } from "./ReceitasPages"

vi.mock("../contexts/AppContext", () => ({
  useApp: vi.fn(),
}))

const products: Product[] = [
  {
    id: 1,
    code: "PA-01",
    name: "Produto acabado",
    type: "PA",
    unit: "un",
    currentStock: 0,
    reservedStock: 0,
    minStock: 0,
    usuarioId: 1,
    createdAt: "",
    updatedAt: "",
  },
  {
    id: 2,
    code: "MP-01",
    name: "Matéria-prima",
    type: "MP",
    unit: "kg",
    currentStock: 0,
    reservedStock: 0,
    minStock: 0,
    usuarioId: 1,
    createdAt: "",
    updatedAt: "",
  },
]

describe("BOM form", () => {
  const addBOM = vi.fn().mockResolvedValue(undefined)

  beforeEach(() => {
    addBOM.mockClear()
    vi.mocked(useApp).mockReturnValue({
      products,
      boms: [],
      addBOM,
    } as unknown as ReturnType<typeof useApp>)
  })

  it("validates the selected finished product and material before creating a BOM", async () => {
    const user = userEvent.setup()
    render(<BOMRegister />)

    await user.click(screen.getByRole("button", { name: "Cadastrar Receita" }))
    expect(screen.getByText("Selecione um produto acabado")).toBeInTheDocument()
    expect(addBOM).not.toHaveBeenCalled()

    await user.click(screen.getByRole("combobox", { name: /Produto Acabado/ }))
    await user.click(await screen.findByRole("option", { name: /PA-01 - Produto acabado/ }))
    await user.click(screen.getByRole("combobox", { name: /Material 1/ }))
    await user.click(await screen.findByRole("option", { name: /MP-01 - Matéria-prima/ }))
    await user.type(screen.getByLabelText(/Quantidade/), "2.5")

    await user.click(screen.getByRole("button", { name: "Cadastrar Receita" }))

    await waitFor(() => {
      expect(addBOM).toHaveBeenCalledWith({
        productId: 1,
        materials: [{ materialId: 2, quantity: 2.5 }],
      })
    })
  })
})
