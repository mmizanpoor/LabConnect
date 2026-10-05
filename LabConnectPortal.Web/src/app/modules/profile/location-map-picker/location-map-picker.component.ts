import {
  AfterViewInit,
  Component,
  ElementRef,
  EventEmitter,
  Input,
  OnChanges,
  OnDestroy,
  Output,
  SimpleChanges,
  ViewChild,
} from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { TranslocoPipe } from '@jsverse/transloco';
import * as L from 'leaflet';

const DEFAULT_LAT = 35.709039;
const DEFAULT_LNG = 51.373141;
const DEFAULT_ZOOM = 16;

const markerIcon = L.icon({
  iconUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon.png',
  iconRetinaUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon-2x.png',
  shadowUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-shadow.png',
  iconSize: [25, 41],
  iconAnchor: [12, 41],
  popupAnchor: [1, -34],
  shadowSize: [41, 41],
});

interface NominatimResult {
  place_id: number;
  display_name: string;
  lat: string;
  lon: string;
}

@Component({
  selector: 'app-location-map-picker',
  standalone: true,
  imports: [TranslocoPipe, MatIconModule],
  templateUrl: './location-map-picker.component.html',
  styleUrl: './location-map-picker.component.scss',
  host: {
    class: 'location-map-picker-host',
    '[class.location-map-picker--fill]': 'fill',
  },
})
export class LocationMapPickerComponent implements AfterViewInit, OnChanges, OnDestroy {
  @ViewChild('mapContainer', { static: true }) private _mapContainer!: ElementRef<HTMLDivElement>;

  @Input() latitude: number | null = null;
  @Input() longitude: number | null = null;
  @Input() disabled = false;
  @Input() compact = false;
  /** When true, map canvas expands to fill remaining vertical space. */
  @Input() fill = false;

  @Output() locationChange = new EventEmitter<{ latitude: number; longitude: number }>();

  searching = false;
  locating = false;
  searchError = '';
  searchResults: NominatimResult[] = [];

  private _map?: L.Map;
  private _marker?: L.Marker;

  ngAfterViewInit(): void {
    this.initMap();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if ((changes['latitude'] || changes['longitude']) && this._map && this._marker) {
      this.setMarkerPosition(this.resolveLat(), this.resolveLng(), false);
    }

    if (changes['disabled'] && this._map) {
      this.applyDisabledState();
    }
  }

  ngOnDestroy(): void {
    this._map?.remove();
    this._map = undefined;
    this._marker = undefined;
  }

  async searchAddress(query: string): Promise<void> {
    if (this.disabled) return;
    const q = query.trim();
    if (!q) return;

    this.searching = true;
    this.searchError = '';
    this.searchResults = [];
    try {
      const url =
        `https://nominatim.openstreetmap.org/search?format=json&limit=5&accept-language=fa&q=` +
        encodeURIComponent(q);
      const response = await fetch(url, {
        headers: { Accept: 'application/json' },
      });
      if (!response.ok) throw new Error('search failed');
      const data = (await response.json()) as NominatimResult[];
      this.searchResults = data ?? [];
      if (!this.searchResults.length) {
        this.searchError = 'آدرسی یافت نشد.';
      }
    } catch {
      this.searchError = 'جستجوی آدرس انجام نشد.';
    } finally {
      this.searching = false;
    }
  }

  selectSearchResult(item: NominatimResult): void {
    const lat = Number(item.lat);
    const lng = Number(item.lon);
    if (!Number.isFinite(lat) || !Number.isFinite(lng)) return;
    this.searchResults = [];
    this.setMarkerPosition(lat, lng, true);
  }

  refreshMapSize(): void {
    setTimeout(() => this._map?.invalidateSize(), 0);
  }

  useCurrentLocation(): void {
    if (this.disabled || !navigator.geolocation) {
      this.searchError = 'موقعیت‌یابی در این مرورگر پشتیبانی نمی‌شود.';
      return;
    }

    this.locating = true;
    this.searchError = '';
    navigator.geolocation.getCurrentPosition(
      (position) => {
        this.locating = false;
        this.setMarkerPosition(position.coords.latitude, position.coords.longitude, true);
      },
      () => {
        this.locating = false;
        this.searchError = 'دسترسی به موقعیت فعلی ممکن نشد.';
      },
      { enableHighAccuracy: true, timeout: 12000 },
    );
  }

  private initMap(): void {
    const lat = this.resolveLat();
    const lng = this.resolveLng();

    this._map = L.map(this._mapContainer.nativeElement, {
      center: [lat, lng],
      zoom: DEFAULT_ZOOM,
      zoomControl: true,
    });

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '&copy; OpenStreetMap',
      maxZoom: 19,
    }).addTo(this._map);

    this._marker = L.marker([lat, lng], { icon: markerIcon, draggable: !this.disabled }).addTo(
      this._map,
    );

    this._marker.on('dragend', () => {
      const position = this._marker?.getLatLng();
      if (!position) return;
      this.emitLocation(position.lat, position.lng);
    });

    this._map.on('click', (event: L.LeafletMouseEvent) => {
      if (this.disabled) return;
      this.setMarkerPosition(event.latlng.lat, event.latlng.lng, true);
    });

    this.applyDisabledState();

    if (this.latitude == null || this.longitude == null) {
      this.emitLocation(lat, lng);
    }

    setTimeout(() => this._map?.invalidateSize(), 0);
  }

  private setMarkerPosition(lat: number, lng: number, emit: boolean): void {
    if (!this._map || !this._marker) return;

    const point = L.latLng(lat, lng);
    this._marker.setLatLng(point);
    this._map.setView(point, Math.max(this._map.getZoom(), 14));

    if (emit) this.emitLocation(lat, lng);
  }

  private emitLocation(lat: number, lng: number): void {
    this.locationChange.emit({
      latitude: Number(lat.toFixed(6)),
      longitude: Number(lng.toFixed(6)),
    });
  }

  private resolveLat(): number {
    return this.latitude ?? DEFAULT_LAT;
  }

  private resolveLng(): number {
    return this.longitude ?? DEFAULT_LNG;
  }

  private applyDisabledState(): void {
    if (this.disabled) {
      this._marker?.dragging?.disable();
    } else {
      this._marker?.dragging?.enable();
    }
  }
}
