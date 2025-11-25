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
        paddingLeft: '24px',
        paddingRight: '24px',
        background: 'linear-gradient(90deg, rgba(212, 163, 115, 0.08) 0%, rgba(139, 69, 19, 0.08) 100%)',
        borderBottom: '1px solid rgba(139, 69, 19, 0.1)'
      }}
    >
      <Image
        src="/logo.png"
        alt="Brewed Logo"
        h={80}
        style={{
          cursor: 'pointer',
          transition: 'transform 0.2s ease',
          borderRadius: '50%',
          objectFit: 'cover'
        }}
        onMouseEnter={(e) => e.currentTarget.style.transform = 'scale(1.05)'}
        onMouseLeave={(e) => e.currentTarget.style.transform = 'scale(1)'}
        onClick={() => navigate('/app/dashboard')}
      />

      <Box style={{ display: 'flex', alignItems: 'center', gap: '24px' }}>
        <Box
          style={{
            position: 'relative',
            cursor: 'pointer',
            padding: '8px',
            borderRadius: '8px',
            transition: 'all 0.2s ease'
          }}
          onMouseEnter={(e) => {
            e.currentTarget.style.background = 'rgba(139, 69, 19, 0.1)';
            e.currentTarget.style.transform = 'scale(1.05)';
          }}
          onMouseLeave={(e) => {
            e.currentTarget.style.background = 'transparent';
            e.currentTarget.style.transform = 'scale(1)';
          }}
          onClick={() => navigate('/app/cart')}
        >
          <IconShoppingCart size={26} color="#8B4513" />
          {cartItemCount > 0 && (
            <Badge
              size="sm"
              circle
              style={{
                position: 'absolute',
                top: 2,
                right: 2,
                padding: 0,
                minWidth: 20,
                height: 20,
                background: 'linear-gradient(135deg, #D4A373 0%, #8B4513 100%)',
                border: '2px solid white',
                boxShadow: '0 2px 4px rgba(0,0,0,0.1)'
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