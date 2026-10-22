export interface LoginResponse { accessToken: string; expiresAt: string; email: string; role: string; }
export interface Product { id: string; sku: string; name: string; description: string | null; unitPrice: number; minimumStock: number; isActive: boolean; }
export interface InventoryItem { productId: string; sku: string; name: string; unitPrice: number; minimumStock: number; currentStock: number; status: 'Disponible' | 'Stock bajo' | 'Sin existencias'; }
export interface Movement { id: string; productId: string; sku: string; productName: string; type: 'Entrada' | 'Salida'; quantity: number; note: string | null; performedBy: string; createdAt: string; }
