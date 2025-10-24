import { Box, Burger, Flex, Image, Badge, Text } from "@mantine/core";
import { IconShoppingCart } from "@tabler/icons-react";
import { useNavigate } from "react-router-dom";
import UserMenuDropdown from "./UserMenuDropdown";
import useCart from "../../hooks/useCart";

const Header = ({ opened, toggle }: any) => {
  const navigate = useNavigate();
  const { cartItemCount } = useCart();

  return (
    <Flex
      justify="space-between"
      align="center"
      style={{ 
        height: "100%", 
        paddingLeft: '20px', 
        paddingRight: '20px',
        background: 'linear-gradient(90deg, rgba(212, 163, 115, 0.1) 0%, rgba(139, 69, 19, 0.1) 100%)'
      }}
    >
      <Image
        src="/logo.png"
        alt="Brewed Logo"
        w={100}
        style={{ cursor: 'pointer' }}
        onClick={() => navigate('/app/dashboard')}
      />

      <Box style={{ display: 'flex', alignItems: 'center', gap: '20px' }}>
        <Box
          style={{ position: 'relative', cursor: 'pointer' }}
          onClick={() => navigate('/app/cart')}
        >
          <IconShoppingCart size={24} color="#8B4513" />
          {cartItemCount > 0 && (
            <Badge
              size="sm"
              circle
              style={{
                position: 'absolute',
                top: -8,
                right: -8,
                padding: 0,
                minWidth: 20,
                height: 20,
                background: 'linear-gradient(135deg, #D4A373 0%, #8B4513 100%)',
                border: 'none'
              }}
            >
              {cartItemCount}
            </Badge>
          )}
        </Box>
        <UserMenuDropdown />
      </Box>

      <Burger opened={opened} onClick={toggle} hiddenFrom="sm" size="sm" color="#8B4513" />
    </Flex>
  );
};

export default Header;