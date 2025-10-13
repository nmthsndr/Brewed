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
      style={{ background: '#f9f9f9' }}
    >
      <AppShell.Header style={{ boxShadow: '0px 5px 10px 0px rgba(82, 63, 105, 0.05)' }}>
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