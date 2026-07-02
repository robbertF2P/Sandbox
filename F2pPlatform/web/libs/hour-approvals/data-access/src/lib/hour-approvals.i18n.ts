export type HourApprovalsLocale = 'en' | 'nl' | 'es' | 'ja' | 'vi';

export type HourApprovalsLabelBundle = Record<string, string>;

export const HOUR_APPROVALS_CORE_I18N: Record<HourApprovalsLocale, HourApprovalsLabelBundle> = {
  en: {
    'hourApprovals.columns.assignedUser': 'Assigned user',
    'hourApprovals.columns.hoursToGo': 'Hours to go',
    'hourApprovals.columns.plannedStart': 'Planned start',
    'hourApprovals.columns.plannedFinish': 'Planned finish',
  },
  nl: {
    'hourApprovals.columns.assignedUser': 'Toegewezen gebruiker',
    'hourApprovals.columns.hoursToGo': 'Uren te gaan',
    'hourApprovals.columns.plannedStart': 'Geplande start',
    'hourApprovals.columns.plannedFinish': 'Geplande finish',
  },
  es: {
    'hourApprovals.columns.assignedUser': 'Usuario asignado',
    'hourApprovals.columns.hoursToGo': 'Horas restantes',
    'hourApprovals.columns.plannedStart': 'Inicio planificado',
    'hourApprovals.columns.plannedFinish': 'Fin planificado',
  },
  ja: {
    'hourApprovals.columns.assignedUser': '担当者',
    'hourApprovals.columns.hoursToGo': '残り時間',
    'hourApprovals.columns.plannedStart': '予定開始',
    'hourApprovals.columns.plannedFinish': '予定終了',
  },
  vi: {
    'hourApprovals.columns.assignedUser': 'Người được giao',
    'hourApprovals.columns.hoursToGo': 'Giờ còn lại',
    'hourApprovals.columns.plannedStart': 'Ngày bắt đầu dự kiến',
    'hourApprovals.columns.plannedFinish': 'Ngày kết thúc dự kiến',
  },
};

export const ACME_HOUR_APPROVALS_PACK_I18N: Record<HourApprovalsLocale, HourApprovalsLabelBundle> = {
  en: {
    'packs.acme-hour-approvals-v1.columns.sapCostElement': 'SAP cost element',
  },
  nl: {
    'packs.acme-hour-approvals-v1.columns.sapCostElement': 'SAP kostenelement',
  },
  es: {
    'packs.acme-hour-approvals-v1.columns.sapCostElement': 'Elemento de coste SAP',
  },
  ja: {
    'packs.acme-hour-approvals-v1.columns.sapCostElement': 'SAP原価要素',
  },
  vi: {
    'packs.acme-hour-approvals-v1.columns.sapCostElement': 'Yếu tố chi phí SAP',
  },
};

const SUPPORTED_LOCALES = new Set<HourApprovalsLocale>(['en', 'nl', 'es', 'ja', 'vi']);

export function resolveHourApprovalsLocale(preferred?: string | null): HourApprovalsLocale {
  const normalized = (preferred ?? navigator.language ?? 'en').toLowerCase();
  const primary = normalized.split('-')[0] as HourApprovalsLocale;
  return SUPPORTED_LOCALES.has(primary) ? primary : 'en';
}

export function translateHourApprovalsLabel(labelKey: string, locale: HourApprovalsLocale): string {
  return ACME_HOUR_APPROVALS_PACK_I18N[locale][labelKey]
    ?? HOUR_APPROVALS_CORE_I18N[locale][labelKey]
    ?? HOUR_APPROVALS_CORE_I18N.en[labelKey]
    ?? labelKey;
}

export function formatHourApprovalsValue(
  value: string | number | boolean | null | undefined,
  format: string | null | undefined,
  locale: HourApprovalsLocale,
): string {
  if (value === null || value === undefined || value === '') {
    return '—';
  }

  if (format === 'date' && typeof value === 'string') {
    const parsed = new Date(value);
    return Number.isNaN(parsed.getTime()) ? value : new Intl.DateTimeFormat(locale).format(parsed);
  }

  if (typeof value === 'number') {
    if (format === 'percent') {
      return new Intl.NumberFormat(locale, { style: 'percent', maximumFractionDigits: 0 }).format(value / 100);
    }

    if (format === 'integer') {
      return new Intl.NumberFormat(locale, { maximumFractionDigits: 0 }).format(value);
    }

    return new Intl.NumberFormat(locale, { maximumFractionDigits: 1 }).format(value);
  }

  return String(value);
}
