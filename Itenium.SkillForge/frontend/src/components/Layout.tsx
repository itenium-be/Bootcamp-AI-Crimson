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
  Users2,
  ChevronsUpDown,
  Briefcase,
  Search,
  BookOpen,
  GraduationCap,
  Award,
  Settings,
} from 'lucide-react';
import { useAuthStore, useTeamStore, useThemeStore, type Team } from '@/stores';
import { fetchUserTeams } from '@/api/client';

const languages = [
  { code: 'nl', name: 'NL' },
  { code: 'en', name: 'EN' },
];

function TeamSwitcher() {
  const { t } = useTranslation();
  const { isMobile } = useSidebar();
  const { mode, setMode, selectedTeam, setSelectedTeam, teams, isBackOffice } = useTeamStore();
  const [searchQuery, setSearchQuery] = useState('');

  const filteredTeams = teams.filter((team) =>
    team.name.toLowerCase().includes(searchQuery.toLowerCase())
  );

  const displayName = mode === 'backoffice' ? t('app.backoffice') : selectedTeam?.name || '';

  // Disable switcher if user only has access to one team and is not backoffice
  const canSwitch = isBackOffice || teams.length > 1;

  const handleSelectBackOffice = () => {
    setMode('backoffice');
    setSearchQuery('');
  };

  const handleSelectTeam = (team: Team) => {
    setMode('local');
    setSelectedTeam(team);
    setSearchQuery('');
  };

  const buttonContent = (
    <>
      <div className="flex aspect-square size-8 items-center justify-center rounded-lg bg-sidebar-primary text-sidebar-primary-foreground">
        {mode === 'backoffice' ? <Briefcase className="size-4" /> : <Users2 className="size-4" />}
      </div>
      <div className="grid flex-1 text-start text-sm leading-tight">
        <span className="truncate font-semibold">SkillForge</span>
        <span className="truncate text-xs">{displayName}</span>
      </div>
      {canSwitch && <ChevronsUpDown className="ms-auto size-4" />}
    </>
  );

  // If user can only access one team, show static display without dropdown
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
            {isBackOffice && (
              <>
                <DropdownMenuItem
                  onClick={handleSelectBackOffice}
                  className="gap-2 p-2"
                >
                  <div className="flex size-6 items-center justify-center rounded-sm border">
                    <Briefcase className="size-4 shrink-0" />
                  </div>
                  <span className="font-medium">{t('app.backoffice')}</span>
                  {mode === 'backoffice' && (
                    <span className="ml-auto text-xs text-muted-foreground">Active</span>
                  )}
                </DropdownMenuItem>
                <DropdownMenuSeparator />
              </>
            )}
            {isBackOffice && (
              <DropdownMenuLabel className="text-xs text-muted-foreground">
                {t('app.teams')}
              </DropdownMenuLabel>
            )}
            <ScrollArea className="max-h-[200px]">
              {filteredTeams.map((team) => (
                <DropdownMenuItem
                  key={team.id}
                  onClick={() => handleSelectTeam(team)}
                  className="gap-2 p-2"
                >
                  <div className="flex size-6 items-center justify-center rounded-sm border">
                    <Users2 className="size-4 shrink-0" />
                  </div>
                  {team.name}
                  {mode === 'local' && selectedTeam?.id === team.id && (
                    <span className="ml-auto text-xs text-muted-foreground">Active</span>
                  )}
                </DropdownMenuItem>
              ))}
              {filteredTeams.length === 0 && (
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
  const { mode, setTeams } = useTeamStore();

  // Fetch teams on mount
  const { data: teamsData } = useQuery({
    queryKey: ['teams'],
    queryFn: fetchUserTeams,
  });

  useEffect(() => {
    if (teamsData) {
      setTeams(teamsData.teams, teamsData.backOffice);
    }
  }, [teamsData, setTeams]);

  const handleLogout = () => {
    logout();
    router.navigate({ to: '/sign-in' });
  };

  // Common navigation items
  const commonNavItems = [
    { path: '/', icon: LayoutDashboard, label: t('nav.dashboard') },
    { path: '/courses', icon: BookOpen, label: t('nav.courses') },
  ];

  // BackOffice-only navigation items
  const backofficeNavItems = [
    { path: '/admin/users', icon: Users, label: t('nav.users') },
    { path: '/admin/teams', icon: Users2, label: t('nav.teams') },
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
          <TeamSwitcher />
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

          {/* BackOffice-only: Admin section */}
          {mode === 'backoffice' && (
            <SidebarGroup>
              <SidebarGroupLabel>{t('nav.admin')}</SidebarGroupLabel>
              <SidebarGroupContent>
                <SidebarMenu>
                  {backofficeNavItems.map((item) => (
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
