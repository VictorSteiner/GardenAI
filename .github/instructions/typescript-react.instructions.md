---
applyTo: "GardenAI.Frontend/**/*.{ts,tsx},GardenAI.Presentation/ClientApp/**/*.{ts,tsx}"
---

# TypeScript / React Instructions

## TanStack Query for Server State

All data fetching goes through **TanStack Query** (formerly React Query). No raw `fetch()` or `axios` in components.

```typescript
// hooks/usePlantPots.ts
import { useQuery } from '@tanstack/react-query';

export function usePlantPots() {
  return useQuery<PlantPotResponse[], Error>({
    queryKey: ['plant-pots'],
    queryFn: async () => {
      const res = await fetch('/api/pots');
      if (!res.ok) throw new Error(`Error: ${res.status}`);
      return res.json() as Promise<PlantPotResponse[]>;
    },
    staleTime: 30_000,  // 30 seconds
    gcTime: 5 * 60 * 1000,  // 5 minutes (garbage collect after)
  });
}

// In component
export function PlantPotsPage() {
  const { data, isLoading, error } = usePlantPots();
  
  if (isLoading) return <p>Loading...</p>;
  if (error) return <p>Error: {error.message}</p>;
  
  return <ul>{data?.map(pot => <li key={pot.id}>{pot.label}</li>)}</ul>;
}
```

---

## Discriminated Unions for State

Never use scattered `isLoading`, `error`, etc. Use **discriminated unions**:

```typescript
// ? Correct
type PlantPotsState =
  | { status: 'pending' }
  | { status: 'success'; data: PlantPotResponse[] }
  | { status: 'error'; error: Error }
  | { status: 'idle' };

// ? Wrong
interface State {
  isLoading: boolean;
  isError: boolean;
  error: Error | null;
  data: PlantPotResponse[] | null;
}

// Usage with TanStack Query
export function usePlantPotsState(): PlantPotsState {
  const { data, isLoading, error } = usePlantPots();
  
  if (isLoading) return { status: 'pending' };
  if (error) return { status: 'error', error };
  if (!data) return { status: 'idle' };
  return { status: 'success', data };
}

// Component
export function PlantPotsPage() {
  const state = usePlantPotsState();
  
  switch (state.status) {
    case 'pending': return <p>Loading...</p>;
    case 'error': return <p>Error: {state.error.message}</p>;
    case 'success': return <ul>{state.data.map(p => <li key={p.id}>{p.label}</li>)}</ul>;
    case 'idle': return <p>No data</p>;
  }
}
```

---

## Per-Page SignalR Hook

Create a **separate SignalR hook per page** — never share a global connection:

```typescript
// hooks/useSensorHub.ts
import { HubConnectionBuilder, HubConnection } from '@microsoft/signalr';
import { useEffect, useState } from 'react';

interface SensorReading {
  potId: string;
  soilMoisture: number;
  temperatureC: number;
  timestamp: string;
}

export function useSensorHub(onReading: (reading: SensorReading) => void) {
  const [connection, setConnection] = useState<HubConnection | null>(null);

  useEffect(() => {
    const conn = new HubConnectionBuilder()
      .withUrl('/hubs/sensors')
      .withAutomaticReconnect([0, 1000, 5000, 10000])
      .build();

    conn.on('ReceiveSensorReading', (reading: SensorReading) => {
      onReading(reading);
    });

    conn.start().catch(console.error);
    setConnection(conn);

    return () => {
      conn.stop().catch(console.error);
    };
  }, [onReading]);

  return connection;
}

// In component
export function SensorReadingsPage() {
  const [readings, setReadings] = useState<SensorReading[]>([]);
  useSensorHub((reading) => {
    setReadings(prev => [...prev.slice(-99), reading]);  // Keep last 100
  });

  return <div>{/* render readings */}</div>;
}
```

---

## TanStack Router

Use **TanStack Router** (not `react-router-dom`):

```typescript
// routes/router.tsx
import { RootRoute, Route, Router } from '@tanstack/react-router';

const rootRoute = new RootRoute({
  component: Root,
});

const dashboardRoute = new Route({
  getParentRoute: () => rootRoute,
  path: '/',
  component: DashboardPage,
});

const potsRoute = new Route({
  getParentRoute: () => rootRoute,
  path: '/pots',
  component: PlantPotsPage,
});

const potDetailRoute = new Route({
  getParentRoute: () => rootRoute,
  path: '/pots/$potId',
  component: PlantPotDetailPage,
  parseParams: (params) => ({
    potId: params.potId,
  }),
});

const routeTree = rootRoute.addChildren([dashboardRoute, potsRoute, potDetailRoute]);

export const router = new Router({ routeTree });

declare module '@tanstack/react-router' {
  interface Register {
    router: typeof router;
  }
}
```

---

## Strict Typing

- ? Never use `any`
- ? Never use `unknown` without a type guard
- ? Always type API responses with interfaces/types

```typescript
// ? Correct
interface PlantPotResponse {
  id: string;
  label: string;
  soilMoisture: number;
  temperatureC: number;
}

export function usePlantPots() {
  return useQuery<PlantPotResponse[], Error>({
    queryKey: ['plant-pots'],
    queryFn: async () => {
      const res = await fetch('/api/pots');
      if (!res.ok) throw new Error(`${res.status}`);
      return res.json() as Promise<PlantPotResponse[]>;
    },
  });
}

// ? Wrong
export function usePlantPots() {
  return useQuery({
    queryKey: ['plant-pots'],
    queryFn: async () => {
      return (await fetch('/api/pots')).json() as any;  // any!
    },
  });
}
```

---

## API Response Types

Match TypeScript interfaces **exactly** to C# DTOs (camelCase):

**C# DTO:**
```csharp
public sealed record PlantPotResponse(
    Guid Id,
    string Label,
    double SoilMoisture,
    double TemperatureC);
```

**TypeScript Interface:**
```typescript
interface PlantPotResponse {
  id: string;          // camelCase (matches System.Text.Json default)
  label: string;
  soilMoisture: number;
  temperatureC: number;
}
```

---

## Tailwind CSS

- ? Use **utility classes only**
- ? Never inline `style={{}}` for static styles
- ? Use inline styles **only** for dynamic values (CSS custom properties)

```typescript
// ? Correct
export function MoistureBar({ moisture }: { moisture: number }) {
  return (
    <div className="w-full bg-gray-200 rounded-full h-4">
      <div
        className="bg-blue-600 h-4 rounded-full transition-all"
        style={{ width: `${moisture}%` }}  // ? Dynamic value
      />
    </div>
  );
}

// ? Wrong
<div style={{ display: 'flex', flexDirection: 'column' }}>
  {/* Use className="flex flex-col" instead */}
</div>
```

---

## Component Structure

```typescript
// components/PlantPots/PlantPotsPage.tsx
import { usePlantPots } from '../../hooks/usePlantPots';

export function PlantPotsPage() {
  const state = usePlantPotsState();
  
  return (
    <div className="container mx-auto p-4">
      <h1 className="text-3xl font-bold mb-6">Plant Pots</h1>
      <PlantPotsContent state={state} />
    </div>
  );
}

function PlantPotsContent({ state }: { state: PlantPotsState }) {
  switch (state.status) {
    case 'pending':
      return <div className="text-center py-8">Loading...</div>;
    case 'error':
      return <div className="text-red-600 p-4">Error: {state.error.message}</div>;
    case 'success':
      return (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {state.data.map(pot => (
            <PlantPotCard key={pot.id} pot={pot} />
          ))}
        </div>
      );
    case 'idle':
      return <p>No data available</p>;
  }
}
```

---

## No `react-router-dom`

? Don't use `react-router-dom` — we use TanStack Router exclusively.

---

## See Also

- **api-design.instructions.md** – Backend endpoint contracts
- **AGENTS.md** – Frontend stack and project-wide conventions

