import { AppShell } from "@mantine/core";
import { useDisclosure } from "@mantine/hooks";
import Header from "./Header";
import { NavbarMinimal } from "./NavbarMinimal";
import { Outlet } from "react-router-dom";

const BasicLayout = () => {
  const [opened, { toggle }] = useDisclosure();

  return (
    <AppShell
      header={{ height: 80 }}
      navbar={{
        width: 250,
        breakpoint: "sm",
        collapsed: { mobile: !opened },
      }}
      padding="md"
      style={{
        background: 'linear-gradient(135deg, #fafafa 0%, #f5f5f5 100%)'
      }}
    >
      <AppShell.Header style={{
        boxShadow: '0 2px 8px rgba(139, 69, 19, 0.08)',
        borderBottom: 'none'
      }}>
        <Header opened={opened} toggle={toggle} />
      </AppShell.Header>

      <AppShell.Navbar style={{ border: 'none' }}>
        <NavbarMinimal toggle={toggle} />
      </AppShell.Navbar>

      <AppShell.Main>
        <Outlet />
      </AppShell.Main>
    </AppShell>
  );
};

export default BasicLayout;