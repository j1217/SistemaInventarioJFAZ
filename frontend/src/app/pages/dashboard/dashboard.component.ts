import { CommonModule } from '@angular/common';
import { Component, computed, OnInit, signal } from '@angular/core';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';
import { AuthService } from '../../core/auth.service';
import { InventoryApiService } from '../../core/inventory-api.service';
import { InventoryItem, Movement, Product } from '../../core/models';

type MovementKind = 'entries' | 'exits';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
})
export class DashboardComponent implements OnInit {
  readonly inventory = signal<InventoryItem[]>([]);
  readonly products = signal<Product[]>([]);
  readonly movements = signal<Movement[]>([]);
  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly modal = signal<'product' | 'movement' | null>(null);
  readonly movementKind = signal<MovementKind>('entries');
  readonly searchTerm = signal('');
  readonly statusFilter = signal('Todos');
  readonly menuOpen = signal(false);
  readonly alertMessage = signal('');
  readonly alertKind = signal<'success' | 'error'>('success');
  readonly selectedProductId = signal('');

  readonly filteredInventory = computed(() => {
    const term = this.searchTerm().trim().toLocaleLowerCase('es-CO');
    return this.inventory().filter((item) => {
      const matchesSearch = !term || `${item.name} ${item.sku}`.toLocaleLowerCase('es-CO').includes(term);
      const matchesStatus = this.statusFilter() === 'Todos' || item.status === this.statusFilter();
      return matchesSearch && matchesStatus;
    });
  });
  readonly totalUnits = computed(() => this.inventory().reduce((sum, item) => sum + item.currentStock, 0));
  readonly totalValue = computed(() => this.inventory().reduce((sum, item) => sum + item.currentStock * item.unitPrice, 0));
  readonly lowStockItems = computed(() => this.inventory().filter((item) => item.status !== 'Disponible'));
  readonly dateLabel = new Intl.DateTimeFormat('es-CO', { weekday: 'long', day: 'numeric', month: 'long', year: 'numeric' }).format(new Date());
  readonly productForm;
  readonly movementForm;

  constructor(
    formBuilder: FormBuilder,
    private readonly api: InventoryApiService,
    readonly auth: AuthService,
    private readonly router: Router,
  ) {
    this.productForm = formBuilder.nonNullable.group({
      sku: ['', [Validators.required, Validators.maxLength(40)]],
      name: ['', [Validators.required, Validators.maxLength(140)]],
      description: [''],
      unitPrice: [0, [Validators.required, Validators.min(0)]],
      minimumStock: [5, [Validators.required, Validators.min(0)]],
    });
    this.movementForm = formBuilder.nonNullable.group({
      productId: ['', Validators.required],
      quantity: [1, [Validators.required, Validators.min(1)]],
      note: [''],
    });
  }

  ngOnInit(): void { this.refresh(); }

  refresh(): void {
    this.loading.set(true);
    this.api.loadDashboard().pipe(finalize(() => this.loading.set(false))).subscribe({
      next: (result) => {
        this.inventory.set(result.inventory);
        this.products.set(result.products);
        this.movements.set(result.movements);
      },
      error: (error) => this.showAlert(error?.error?.detail ?? 'No fue posible cargar el inventario.', 'error'),
    });
  }

  openProduct(): void {
    this.productForm.reset({ sku: '', name: '', description: '', unitPrice: 0, minimumStock: 5 });
    this.modal.set('product');
  }

  openMovement(kind: MovementKind, productId = ''): void {
    this.movementKind.set(kind);
    this.movementForm.reset({ productId, quantity: 1, note: '' });
    this.selectedProductId.set(productId);
    this.modal.set('movement');
  }

  closeModal(): void { this.modal.set(null); }

  submitProduct(): void {
    if (this.productForm.invalid) {
      this.productForm.markAllAsTouched();
      return;
    }
    this.saving.set(true);
    this.api.createProduct(this.productForm.getRawValue()).pipe(finalize(() => this.saving.set(false))).subscribe({
      next: () => {
        this.closeModal();
        this.showAlert('El producto se agregó al inventario.', 'success');
        this.refresh();
      },
      error: (error) => this.showAlert(error?.error?.detail ?? 'No fue posible guardar el producto.', 'error'),
    });
  }

  submitMovement(): void {
    if (this.movementForm.invalid) {
      this.movementForm.markAllAsTouched();
      return;
    }
    const { productId, quantity, note } = this.movementForm.getRawValue();
    this.saving.set(true);
    this.api.registerMovement(this.movementKind(), productId, quantity, note).pipe(finalize(() => this.saving.set(false))).subscribe({
      next: () => {
        this.closeModal();
        this.showAlert(this.movementKind() === 'entries' ? 'Entrada registrada correctamente.' : 'Salida registrada correctamente.', 'success');
        this.refresh();
      },
      error: (error) => this.showAlert(error?.error?.detail ?? 'No fue posible registrar el movimiento.', 'error'),
    });
  }

  moveTo(section: string): void {
    document.getElementById(section)?.scrollIntoView({ behavior: 'smooth', block: 'start' });
    this.menuOpen.set(false);
  }

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/login']);
  }

  userInitials(): string {
    const email = this.auth.currentUser()?.email ?? 'AD';
    return email.slice(0, 2).toUpperCase();
  }

  displayName(): string {
    return this.auth.currentUser()?.email.split('@')[0] ?? 'equipo';
  }

  currency(value: number): string {
    return new Intl.NumberFormat('es-CO', { style: 'currency', currency: 'COP', maximumFractionDigits: 0 }).format(value);
  }

  dateTime(value: string): string {
    return new Intl.DateTimeFormat('es-CO', { day: '2-digit', month: 'short', hour: '2-digit', minute: '2-digit' }).format(new Date(value));
  }

  stockWidth(item: InventoryItem): number {
    return Math.min(100, Math.max(7, item.currentStock * 5));
  }

  category(sku: string): string { return sku.startsWith('ACC') ? 'Accesorios' : 'Tecnología'; }

  private showAlert(message: string, kind: 'success' | 'error'): void {
    this.alertMessage.set(message);
    this.alertKind.set(kind);
    window.setTimeout(() => this.alertMessage.set(''), 4200);
  }
}
