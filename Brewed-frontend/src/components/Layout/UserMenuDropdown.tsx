import { Avatar, Text, Group, Menu, rem, UnstyledButton } from "@mantine/core";
import { IconChevronDown, IconLogout, IconUserCircle, IconLogin, IconUserPlus } from "@tabler/icons-react";
import { useNavigate } from "react-router-dom";
import useAuth from "../../hooks/useAuth";

const UserMenuDropdown = () => {
  const navigate = useNavigate();
  const { logout, email, isLoggedIn } = useAuth();

  const loggedInItems = [
    {
      label: "Profile",
      onClick: () => navigate('/app/profile'),
      icon: IconUserCircle
    },
    {
      label: "Logout",
      onClick: () => {
        logout();
        navigate('/login');
      },
      icon: IconLogout
    }
  ];

  const guestItems = [
    {
      label: "Login",
      onClick: () => navigate('/login'),
      icon: IconLogin
    },
    {
      label: "Register",
      onClick: () => navigate('/register'),
      icon: IconUserPlus
    }
  ];

  const items = isLoggedIn ? loggedInItems : guestItems;

  return (
    <Menu width={220} position="bottom-end" transitionProps={{ transition: 'pop-top-right' }} shadow="lg" radius="md">
      <Menu.Target>
        <UnstyledButton
          style={{
            padding: '4px 10px',
            borderRadius: '10px',
            transition: 'all 0.2s ease',
          }}
        >
          <Group gap={7}>
            <Avatar
              radius="xl"
              size={32}
              color="brown"
              variant="light"
              style={{
                border: '2px solid rgba(139, 69, 19, 0.15)',
              }}
            />
            <Text fw={500} size="sm" lh={1} mr={3} c="#5c5c5c" visibleFrom="sm">
              {isLoggedIn ? email : 'Guest'}
            </Text>
            <IconChevronDown style={{ width: rem(12), height: rem(12), color: '#8B4513' }} stroke={1.5} />
          </Group>
        </UnstyledButton>
      </Menu.Target>

      <Menu.Dropdown style={{ border: '1px solid rgba(139, 69, 19, 0.1)' }}>
        {items.map(item => (
          <Menu.Item
            key={item.label}
            onClick={item.onClick}
            leftSection={<item.icon style={{ width: rem(16), height: rem(16) }} stroke={1.5} />}
            style={{ borderRadius: '8px' }}
          >
            {item.label}
          </Menu.Item>
        ))}
      </Menu.Dropdown>
    </Menu>
  );
};

export default UserMenuDropdown;