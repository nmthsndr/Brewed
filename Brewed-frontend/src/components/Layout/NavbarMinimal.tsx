import { useEffect, useState } from "react";
import { rem, Button, useMantineTheme } from "@mantine/core";
import {
  IconUserCircle,
  IconLogout,
  IconHome,
  IconShoppingBag,
  IconShoppingCart,
  IconPackage,
  IconStar,
  IconTicket,
  IconGift,
  IconCategory,
  IconUsers,
  IconChartBar,
  IconClipboardList,
  IconLogin
} from "@tabler/icons-react";
import classes from "./NavbarMinimalColored.module.css";
import { useNavigate, useLocation } from "react-router-dom";
import { useMediaQuery } from "@mantine/hooks";
import useAuth from "../../hooks/useAuth";

interface NavbarLinkProps {
  icon: typeof IconHome;
  label: string;
  color: string;
  active?: boolean;
  onClick?(): void;
}

function NavbarLink({ icon: Icon, label, color, active, onClick }: NavbarLinkProps) {
  return (
    <div
      role="button"
      className={classes.link}
      onClick={onClick}
      data-active={active || undefined}
    >
      <Button
        variant="light"
        color={color}
        className={classes.iconButton}
        style={{ width: rem(40), height: rem(35) }}
      >
        <Icon className={classes.linkIcon} style={{ width: rem(25), height: rem(25) }} stroke={1.8} />
      </Button>
      <span>{label}</span>
    </div>
  );
}

export function NavbarMinimal({ toggle }: any) {
  const theme = useMantineTheme();
  const isMobile = useMediaQuery(`(max-width: ${theme.breakpoints.sm})`);
  const [active, setActive] = useState(0);
  const navigate = useNavigate();
  const location = useLocation();
  const { logout, role, isLoggedIn } = useAuth();

  const menuItems = [
    {
      icon: IconHome,
      label: "Dashboard",
      url: "dashboard",
      roles: ['Admin', 'RegisteredUser', 'Guest']
    },
    {
      icon: IconShoppingBag,
      label: "Products",
      url: "products",
      roles: ['Admin', 'RegisteredUser', 'Guest']
    },
    {
      icon: IconCategory,
      label: "Manage Products",
      url: "admin-products",
      roles: ['Admin']
    },
    {
      icon: IconCategory,
      label: "Categories",
      url: "categories",
      roles: ['Admin']
    },
    {
      icon: IconShoppingCart,
      label: "Cart",
      url: "cart",
      roles: ['Admin', 'RegisteredUser', 'Guest']
    },
    {
      icon: IconPackage,
      label: "Orders",
      url: "orders",
      roles: ['Admin','RegisteredUser']
    },
    {
      icon: IconGift,
      label: "My Coupons",
      url: "my-coupons",
      roles: ['Admin','RegisteredUser']
    },
    {
      icon: IconClipboardList,
      label: "All Orders",
      url: "admin-orders",
      roles: ['Admin']
    },
    {
      icon: IconStar,
      label: "Reviews",
      url: "admin-reviews",
      roles: ['Admin']
    },
    {
      icon: IconTicket,
      label: "Coupons",
      url: "coupons",
      roles: ['Admin']
    },
    {
      icon: IconUsers,
      label: "Users",
      url: "users",
      roles: ['Admin']
    },
    {
      icon: IconChartBar,
      label: "Analytics",
      url: "admin-dashboard",
      roles: ['Admin']
    }
  ];

  const onLogout = () => {
    logout();
    navigate('/login');
  };

  useEffect(() => {
    const currentPath = location.pathname.split('/').pop() || '';
    const effectiveRole = isLoggedIn ? role : 'Guest';
    const filteredItems = menuItems.filter(item => effectiveRole && item.roles.includes(effectiveRole));
    setActive(filteredItems.findIndex(m => currentPath === m.url));
  }, [location.pathname, role, isLoggedIn]);

  const effectiveRole = isLoggedIn ? role : 'Guest';
  const links = menuItems
    .filter(item => effectiveRole && item.roles.includes(effectiveRole))
    .map((link, index) => (
      <NavbarLink
        color="blue"
        {...link}
        key={link.label}
        active={index === active}
        onClick={() => {
          setActive(index);
          if (isMobile) toggle();
          navigate(link.url);
        }}
      />
    ));

  return (
    <nav className={classes.navbar}>
      <div>
        <div className={classes.navbarMain}>
          {links}
        </div>
        <div className={classes.footer}>
          {isLoggedIn ? (
            <>
              <NavbarLink
                active={location.pathname.endsWith('profile')}
                icon={IconUserCircle}
                label="Profile"
                onClick={() => {
                  navigate("profile");
                  if (isMobile) toggle();
                }}
                color="blue"
              />
              <NavbarLink
                icon={IconLogout}
                label="Logout"
                onClick={onLogout}
                color="red"
              />
            </>
          ) : (
            <NavbarLink
              icon={IconLogin}
              label="Login"
              onClick={() => {
                navigate("/login");
                if (isMobile) toggle();
              }}
              color="blue"
            />
          )}
        </div>
      </div>
    </nav>
  );
}