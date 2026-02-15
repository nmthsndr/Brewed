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
      }}
    >
      <Box
        style={{
          display: 'flex',
          alignItems: 'center',
          gap: '12px',
          cursor: 'pointer',
        }}
        onClick={() => navigate('/app/dashboard')}
      >
        <Image
          src="/logo.png"
          alt="Brewed Logo"
          h={48}
          w={48}
          style={{
            transition: 'transform 0.25s cubic-bezier(0.4, 0, 0.2, 1)',
            borderRadius: '50%',
            objectFit: 'cover',
            border: '2px solid rgba(139, 69, 19, 0.12)',
          }}
          onMouseEnter={(e) => e.currentTarget.style.transform = 'scale(1.08) rotate(5deg)'}
          onMouseLeave={(e) => e.currentTarget.style.transform = 'scale(1) rotate(0deg)'}
        />
        <Text
          fw={700}
          size="xl"
          style={{
            fontFamily: '"Playfair Display", Georgia, serif',
            color: '#8B4513',
            letterSpacing: '-0.02em',
          }}
          visibleFrom="md"
        >
          Brewed
        </Text>
      </Box>

      <Box style={{ display: 'flex', alignItems: 'center', gap: '16px' }}>
        <Box
          style={{
            position: 'relative',
            cursor: 'pointer',
            padding: '8px',
            borderRadius: '12px',
            transition: 'all 0.2s cubic-bezier(0.4, 0, 0.2, 1)',
          }}
          onMouseEnter={(e) => {
            e.currentTarget.style.background = 'rgba(139, 69, 19, 0.08)';
            e.currentTarget.style.transform = 'scale(1.08)';
          }}
          onMouseLeave={(e) => {
            e.currentTarget.style.background = 'transparent';
            e.currentTarget.style.transform = 'scale(1)';
          }}
          onClick={() => navigate('/app/cart')}
        >
          <IconShoppingCart size={24} color="#8B4513" stroke={1.8} />
          {cartItemCount > 0 && (
            <Badge
              size="xs"
              circle
              style={{
                position: 'absolute',
                top: 2,
                right: 2,
                padding: 0,
                minWidth: 18,
                height: 18,
                fontSize: '0.65rem',
                background: 'linear-gradient(135deg, #D4A373 0%, #8B4513 100%)',
                border: '2px solid white',
                boxShadow: '0 2px 6px rgba(139, 69, 19, 0.25)',
              }}
            >
              {cartItemCount}
            </Badge>
          )}
        </Box>

        <Box
          style={{
            width: '1px',
            height: '24px',
            background: 'rgba(139, 69, 19, 0.12)',
          }}
          visibleFrom="md"
        />

        <UserMenuDropdown />
      </Box>

      <Burger opened={opened} onClick={toggle} hiddenFrom="md" size="sm" color="#8B4513" />
    </Flex>
  );
};

export default Header;