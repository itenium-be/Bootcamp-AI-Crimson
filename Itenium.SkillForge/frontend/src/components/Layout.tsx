import { useState, useEffect } from 'react';
import { Outlet, Link, useRouter } from '@tanstack/react-router';
import { useTranslation } from 'react-i18next';
import { useQuery } from '@tanstack/react-query';
import {
  SidebarProvider,
  Sidebar,
  SidebarHeader,
  SidebarContent,
  SidebarFooter,
  SidebarMenu,
  SidebarMenuItem,
  SidebarMenuButton,
  SidebarGroup,
  SidebarGroupLabel,
  SidebarGroupContent,
  SidebarInset,
  SidebarTrigger,
  useSidebar,
  Button,
  Input,
  DropdownMenu,
  DropdownMenuTrigger,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  Avatar,
  AvatarFallback,
  ScrollArea,
} from '@itenium-forge/ui';
import {
  LayoutDashboard,
  Users,
  LogOut,
  Sun,
  Moon,
  Building2,
  ChevronsUpDown,
  Globe,
  Search,
  BookOpen,
  GraduationCap,
  Award,
  Settings,
} from 'lucide-react';
import { useAuthStore, useOrganizationStore, useThemeStore, type Organization } from '@/stores';
import { fetchUserOrganizations } from '@/api/client';

const languages = [
  { code: 'nl', name: 'NL' },
  { code: 'en', name: 'EN' },
];

function OrganizationSwitcher() {
  const { t } = useTranslation();
  const { isMobile } = useSidebar();
  const { mode, setMode, selectedOrganization, setSelectedOrganization, organizations, isCentral } = useOrganizationStore();
  const [searchQuery, setSearchQuery] = useState('');

  const filteredOrganizations = organizations.filter((org) =>
    org.name.toLowerCase().includes(searchQuery.toLowerCase())
  );

  const displayName = mode === 'central' ? t('app.central') : selectedOrganization?.name || '';

  // Disable switcher if user only has access to one organization and is not central
  const canSwitch = isCentral || organizations.length > 1;

  const handleSelectCentral = () => {
    setMode('central');
    setSearchQuery('');
  };

  const handleSelectOrganization = (organization: Organization) => {
    setMode('local');
    setSelectedOrganization(organization);
    setSearchQuery('');
  };

  const buttonContent = (
    <>
      <div className="flex aspect-square size-8 items-center justify-center rounded-lg bg-sidebar-primary text-sidebar-primary-foreground">
        {mode === 'central' ? <Globe className="size-4" /> : <Building2 className="size-4" />}
      </div>
      <div className="grid flex-1 text-start text-sm leading-tight">
        <span className="truncate font-semibold">SkillForge</span>
        <span className="truncate text-xs">{displayName}</span>
      </div>
      {canSwitch && <ChevronsUpDown className="ms-auto size-4" />}
    </>
  );

  // If user can only access one organization, show static display without dropdown
  if (!canSwitch) {
    return (
      <SidebarMenu>
        <SidebarMenuItem>
          <SidebarMenuButton
            size="lg"
            className="cursor-default"
          >
            {buttonContent}
          </SidebarMenuButton>
        </SidebarMenuItem>
      </SidebarMenu>
    );
  }

  return (
    <SidebarMenu>
      <SidebarMenuItem>
        <DropdownMenu>
          <DropdownMenuTrigger asChild>
            <SidebarMenuButton
              size="lg"
              className="data-[state=open]:bg-sidebar-accent data-[state=open]:text-sidebar-accent-foreground"
            >
              {buttonContent}
            </SidebarMenuButton>
          </DropdownMenuTrigger>
          <DropdownMenuContent
            className="w-[--radix-dropdown-menu-trigger-width] min-w-56 rounded-lg"
            align="start"
            side={isMobile ? 'bottom' : 'right'}
            sideOffset={4}
          >
            <div className="p-2">
              <div className="relative">
                <Search className="absolute left-2 top-1/2 -translate-y-1/2 size-4 text-muted-foreground" />
                <Input
                  placeholder={t('common.search')}
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)}
                  onKeyDown={(e) => e.stopPropagation()}
                  className="pl-8 h-8"
                />
              </div>
            </div>
            <DropdownMenuSeparator />
            {isCentral && (
              <>
                <DropdownMenuItem
                  onClick={handleSelectCentral}
                  className="gap-2 p-2"
                >
                  <div className="flex size-6 items-center justify-center rounded-sm border">
                    <Globe className="size-4 shrink-0" />
                  </div>
                  <span className="font-medium">{t('app.central')}</span>
                  {mode === 'central' && (
                    <span className="ml-auto text-xs text-muted-foreground">Active</span>
                  )}
                </DropdownMenuItem>
                <DropdownMenuSeparator />
              </>
            )}
            {isCentral && (
              <DropdownMenuLabel className="text-xs text-muted-foreground">
                {t('app.local')}
              </DropdownMenuLabel>
            )}
            <ScrollArea className="max-h-[200px]">
              {filteredOrganizations.map((org) => (
                <DropdownMenuItem
                  key={org.id}
                  onClick={() => handleSelectOrganization(org)}
                  className="gap-2 p-2"
                >
                  <div className="flex size-6 items-center justify-center rounded-sm border">
                    <Building2 className="size-4 shrink-0" />
                  </div>
                  {org.name}
                  {mode === 'local' && selectedOrganization?.id === org.id && (
                    <span className="ml-auto text-xs text-muted-foreground">Active</span>
                  )}
                </DropdownMenuItem>
              ))}
              {filteredOrganizations.length === 0 && (
                <div className="p-2 text-sm text-muted-foreground text-center">
                  {t('common.noResults')}
                </div>
              )}
            </ScrollArea>
          </DropdownMenuContent>
        </DropdownMenu>
      </SidebarMenuItem>
    </SidebarMenu>
  );
}

export function Layout() {
  const { t, i18n } = useTranslation();
  const router = useRouter();
  const { user, logout } = useAuthStore();
  const { resolvedTheme, setTheme } = useThemeStore();
  const { mode, setOrganizations } = useOrganizationStore();

  // Fetch organizations on mount
  const { data: organizationsData } = useQuery({
    queryKey: ['organizations'],
    queryFn: fetchUserOrganizations,
  });

  useEffect(() => {
    if (organizationsData) {
      setOrganizations(organizationsData.organizations, organizationsData.central);
    }
  }, [organizationsData, setOrganizations]);

  const handleLogout = () => {
    logout();
    router.navigate({ to: '/sign-in' });
  };

  // Common navigation items
  const commonNavItems = [
    { path: '/', icon: LayoutDashboard, label: t('nav.dashboard') },
    { path: '/courses', icon: BookOpen, label: t('nav.courses') },
  ];

  // Central-only navigation items
  const centralNavItems = [
    { path: '/admin/users', icon: Users, label: t('nav.users') },
    { path: '/admin/organizations', icon: Building2, label: t('nav.organizations') },
  ];

  // Local-only navigation items
  const localNavItems = [
    { path: '/enrollments', icon: GraduationCap, label: t('nav.enrollments') },
    { path: '/progress', icon: Award, label: t('nav.progress') },
  ];

  return (
    <SidebarProvider>
      <Sidebar>
        <SidebarHeader>
          <OrganizationSwitcher />
        </SidebarHeader>

        <SidebarContent>
          {/* Common navigation items */}
          <SidebarGroup>
            <SidebarGroupLabel>{t('nav.navigation')}</SidebarGroupLabel>
            <SidebarGroupContent>
              <SidebarMenu>
                {commonNavItems.map((item) => (
                  <SidebarMenuItem key={item.path}>
                    <SidebarMenuButton asChild>
                      <Link
                        to={item.path}
                        activeProps={{ className: 'bg-accent' }}
                      >
                        <item.icon className="size-4" />
                        <span>{item.label}</span>
                      </Link>
                    </SidebarMenuButton>
                  </SidebarMenuItem>
                ))}
              </SidebarMenu>
            </SidebarGroupContent>
          </SidebarGroup>

          {/* Central-only: Admin section */}
          {mode === 'central' && (
            <SidebarGroup>
              <SidebarGroupLabel>{t('nav.admin')}</SidebarGroupLabel>
              <SidebarGroupContent>
                <SidebarMenu>
                  {centralNavItems.map((item) => (
                    <SidebarMenuItem key={item.path}>
                      <SidebarMenuButton asChild>
                        <Link
                          to={item.path}
                          activeProps={{ className: 'bg-accent' }}
                        >
                          <item.icon className="size-4" />
                          <span>{item.label}</span>
                        </Link>
                      </SidebarMenuButton>
                    </SidebarMenuItem>
                  ))}
                </SidebarMenu>
              </SidebarGroupContent>
            </SidebarGroup>
          )}

          {/* Local-only navigation items */}
          {mode === 'local' && (
            <SidebarGroup>
              <SidebarGroupLabel>{t('nav.operations')}</SidebarGroupLabel>
              <SidebarGroupContent>
                <SidebarMenu>
                  {localNavItems.map((item) => (
                    <SidebarMenuItem key={item.path}>
                      <SidebarMenuButton asChild>
                        <Link
                          to={item.path}
                          activeProps={{ className: 'bg-accent' }}
                        >
                          <item.icon className="size-4" />
                          <span>{item.label}</span>
                        </Link>
                      </SidebarMenuButton>
                    </SidebarMenuItem>
                  ))}
                </SidebarMenu>
              </SidebarGroupContent>
            </SidebarGroup>
          )}
        </SidebarContent>

        <SidebarFooter>
          <SidebarMenu>
            <SidebarMenuItem>
              <DropdownMenu>
                <DropdownMenuTrigger asChild>
                  <SidebarMenuButton className="w-full">
                    <Avatar className="size-6">
                      <AvatarFallback>
                        {user?.name?.charAt(0).toUpperCase() || 'U'}
                      </AvatarFallback>
                    </Avatar>
                    <span className="flex-1 text-left truncate">
                      {user?.name || 'User'}
                    </span>
                  </SidebarMenuButton>
                </DropdownMenuTrigger>
                <DropdownMenuContent side="top" align="start" className="w-56">
                  <DropdownMenuItem asChild>
                    <Link to="/settings">
                      <Settings className="size-4 mr-2" />
                      {t('nav.settings')}
                    </Link>
                  </DropdownMenuItem>
                  <DropdownMenuSeparator />
                  <DropdownMenuItem onClick={handleLogout}>
                    <LogOut className="size-4 mr-2" />
                    {t('nav.signOut')}
                  </DropdownMenuItem>
                </DropdownMenuContent>
              </DropdownMenu>
            </SidebarMenuItem>
          </SidebarMenu>
        </SidebarFooter>
      </Sidebar>

      <SidebarInset>
        <header className="flex h-14 items-center justify-between px-4">
          <div className="flex items-center gap-2">
            <SidebarTrigger />
          </div>

          <div className="flex items-center gap-2">
            {/* Language Switcher */}
            <div className="flex items-center gap-1">
              {languages.map((lang) => (
                <Button
                  key={lang.code}
                  variant={i18n.language === lang.code ? 'default' : 'ghost'}
                  size="sm"
                  onClick={() => {
                    i18n.changeLanguage(lang.code);
                    localStorage.setItem('language', lang.code);
                  }}
                >
                  {lang.name}
                </Button>
              ))}
            </div>

            {/* Theme Toggle */}
            <Button
              variant="ghost"
              size="sm"
              onClick={() => setTheme(resolvedTheme === 'dark' ? 'light' : 'dark')}
            >
              {resolvedTheme === 'dark' ? (
                <Sun className="size-4" />
              ) : (
                <Moon className="size-4" />
              )}
            </Button>
          </div>
        </header>

        <main className="flex-1 p-6">
          <Outlet />
        </main>
      </SidebarInset>
    </SidebarProvider>
  );
}
