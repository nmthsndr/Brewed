import { Avatar, Text, Group, Menu, rem, UnstyledButton } from "@mantine/core";
import { IconChevronDown, IconLogout, IconUserCircle } from "@tabler/icons-react";
import { useNavigate } from "react-router-dom";
import useAuth from "../../hooks/useAuth";

const UserMenuDropdown = () => {
  const navigate = useNavigate();
  const { logout, email } = useAuth();

  const items = [
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

  return (
    <Menu width={260} position="bottom-end" transitionProps={{ transition: 'pop-top-right' }}>
      <Menu.Target>
        <UnstyledButton>
          <Group gap={7}>
            <Avatar radius="xl" size={30} color="blue" />
            <Text fw={500} size="sm" lh={1} mr={3}>
              {email || 'User'}
            </Text>
            <IconChevronDown style={{ width: rem(12), height: rem(12) }} stroke={1.5} />
          </Group>
        </UnstyledButton>
      </Menu.Target>

      <Menu.Dropdown>
        {items.map(item => (
          <Menu.Item
            key={item.label}
            onClick={item.onClick}
            leftSection={<item.icon style={{ width: rem(16), height: rem(16) }} stroke={1.5} />}
          >
            {item.label}
          </Menu.Item>
        ))}
      </Menu.Dropdown>
    </Menu>
  );
};

export default UserMenuDropdown;