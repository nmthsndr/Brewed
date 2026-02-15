import { AppShell } from "@mantine/core";
import { useDisclosure } from "@mantine/hooks";
import Header from "./Header";
import { NavbarMinimal } from "./NavbarMinimal";
import Footer from "./Footer";
import { Outlet } from "react-router-dom";

const BasicLayout = () => {
  const [opened, { toggle }] = useDisclosure();

  return (
    <AppShell
      header={{ height: 72 }}
      footer={{ height: 60 }}
      navbar={{
        width: 250,
        breakpoint: "sm",
        collapsed: { mobile: !opened },
      }}
      padding="md"
      style={{
        background: 'linear-gradient(160deg, #faf8f5 0%, #f3ede6 50%, #faf8f5 100%)',
      }}
    >
      <AppShell.Header style={{
        boxShadow: '0 1px 12px rgba(139, 69, 19, 0.06)',
        borderBottom: '1px solid rgba(139, 69, 19, 0.06)',
        backdropFilter: 'blur(12px)',
        background: 'rgba(255, 255, 255, 0.92)',
      }}>
        <Header opened={opened} toggle={toggle} />
      </AppShell.Header>

      <AppShell.Navbar style={{ border: 'none' }}>
        <NavbarMinimal toggle={toggle} />
      </AppShell.Navbar>

      <AppShell.Main>
        <Outlet />
      </AppShell.Main>

      <AppShell.Footer style={{
        borderTop: '1px solid rgba(139, 69, 19, 0.1)',
        background: 'rgba(255, 255, 255, 0.92)',
        backdropFilter: 'blur(12px)',
      }}>
        <Footer />
      </AppShell.Footer>
    </AppShell>
  );
};

export default BasicLayout;