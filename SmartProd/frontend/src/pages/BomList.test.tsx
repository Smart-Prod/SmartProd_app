import { render, screen } from "@testing-library/react"
import { beforeEach, describe, expect, it, vi } from "vitest"
import { useApp } from "../contexts/AppContext"
import type { BOM, Product } from "../models"
import { BomList } from "./BomList"

vi.mock("../contexts/AppContext", () => ({
  useApp: vi.fn(),
}))

const product = (id: number, name: string, type: Product["type"], unit: string): Product => ({
  id,
  code: `SKU-${id}`,
  name,
  type,
  unit,
  currentStock: 0,
  reservedStock: 0,
  minStock: 0,
  usuarioId: 1,
  createdAt: "",
  updatedAt: "",
})

describe("BomList", () => {
  beforeEach(() => {
    vi.mocked(useApp).mockReturnValue({ boms: [] } as unknown as ReturnType<typeof useApp>)
  })

  it("shows an empty-state message when there are no BOMs", () => {
    render(<BomList />)

    expect(screen.getByText("Nenhuma estrutura de produto cadastrada.")).toBeInTheDocument()
    expect(screen.queryByRole("table")).not.toBeInTheDocument()
  })

  it("renders products and their raw materials", () => {
    const finishedProduct = product(1, "Produto final", "PA", "un")
    const rawMaterial = product(2, "Aço", "MP", "kg")
    const bom: BOM = {
      id: 10,
      productId: finishedProduct.id,
      product: finishedProduct,
      materials: [
        {
          id: 20,
          bomId: 10,
          materialId: rawMaterial.id,
          material: rawMaterial,
          quantity: 2.5,
        },
      ],
      createdAt: "",
      updatedAt: "",
    }
    vi.mocked(useApp).mockReturnValue({ boms: [bom] } as ReturnType<typeof useApp>)

    render(<BomList />)

    expect(screen.getByRole("table")).toBeInTheDocument()
    expect(screen.getByText("Produto final")).toBeInTheDocument()
    expect(screen.getByText("Aço — 2.5 kg")).toBeInTheDocument()
  })
})
