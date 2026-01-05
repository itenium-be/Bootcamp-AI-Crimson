import { useTranslation } from 'react-i18next';

export function Settings() {
  const { t } = useTranslation();

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">{t('settings.title')}</h1>
      </div>
      <p className="text-muted-foreground">Settings page coming soon...</p>
    </div>
  );
}
