import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { forkJoin, Observable } from 'rxjs';
import { InventoryItem, Movement, Product } from './models';

const API_URL = 'http://localhost:5080/api';

@Injectable({ providedIn: 'root' })
export class InventoryApiService {
  constructor(private readonly http: HttpClient) {}

  loadDashboard(): Observable<{ inventory: InventoryItem[]; products: Product[]; movements: Movement[] }> {
    return forkJoin({
      inventory: this.http.get<InventoryItem[]>(`${API_URL}/inventory`),
      products: this.http.get<Product[]>(`${API_URL}/products`),
      movements: this.http.get<Movement[]>(`${API_URL}/inventory/movements?take=8`),
    });
  }

  createProduct(product: Omit<Product, 'id' | 'isActive'>): Observable<Product> {
    return this.http.post<Product>(`${API_URL}/products`, product);
  }

  registerMovement(type: 'entries' | 'exits', productId: string, quantity: number, note: string): Observable<unknown> {
    return this.http.post(`${API_URL}/inventory/${type}`, { productId, quantity, note });
  }
}
