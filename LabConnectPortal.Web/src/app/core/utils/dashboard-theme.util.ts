export type DashboardTheme = 'blue' | 'green' | 'violet' | 'amber' | 'slate' | 'rose';

export interface DashboardThemeClasses {
  cardBorder: string;
  icon: string;
  iconColor: string;
  quickLinkIcon: string;
}

const THEME_MAP: Record<DashboardTheme, DashboardThemeClasses> = {
  blue: {
    cardBorder: 'border-t-blue-600/40',
    icon: 'bg-blue-50 text-blue-600',
    iconColor: 'text-blue-600',
    quickLinkIcon: 'bg-blue-50 text-blue-600',
  },
  green: {
    cardBorder: 'border-t-emerald-600/40',
    icon: 'bg-emerald-50 text-emerald-600',
    iconColor: 'text-emerald-600',
    quickLinkIcon: 'bg-emerald-50 text-emerald-600',
  },
  violet: {
    cardBorder: 'border-t-violet-600/40',
    icon: 'bg-violet-50 text-violet-600',
    iconColor: 'text-violet-600',
    quickLinkIcon: 'bg-violet-50 text-violet-600',
  },
  amber: {
    cardBorder: 'border-t-amber-500/40',
    icon: 'bg-amber-50 text-amber-600',
    iconColor: 'text-amber-600',
    quickLinkIcon: 'bg-amber-50 text-amber-600',
  },
  slate: {
    cardBorder: 'border-t-slate-500/40',
    icon: 'bg-slate-100 text-slate-600',
    iconColor: 'text-slate-600',
    quickLinkIcon: 'bg-slate-100 text-slate-600',
  },
  rose: {
    cardBorder: 'border-t-rose-600/40',
    icon: 'bg-rose-50 text-rose-600',
    iconColor: 'text-rose-600',
    quickLinkIcon: 'bg-rose-50 text-rose-600',
  },
};

export function dashboardThemeClasses(theme: DashboardTheme): DashboardThemeClasses {
  return THEME_MAP[theme];
}
