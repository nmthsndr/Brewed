import { Card, Image, Text, Badge, Button, Group, Stack } from "@mantine/core";
import { IconShoppingCart, IconStar } from "@tabler/icons-react";
import { IProduct } from "../interfaces/IProduct";
import { useNavigate } from "react-router-dom";

interface ProductCardProps {
  product: IProduct;
  onAddToCart?: (productId: number) => void;
}

const ProductCard = ({ product, onAddToCart }: ProductCardProps) => {
  const navigate = useNavigate();

  return (
    <Card 
      shadow="sm" 
      padding="lg" 
      radius="md" 
      withBorder
      style={{
        borderColor: '#D4A373',
        transition: 'all 0.3s ease',
        cursor: 'pointer'
      }}
      onMouseEnter={(e) => {
        e.currentTarget.style.transform = 'translateY(-5px)';
        e.currentTarget.style.boxShadow = '0 8px 16px rgba(139, 69, 19, 0.2)';
      }}
      onMouseLeave={(e) => {
        e.currentTarget.style.transform = 'translateY(0)';
        e.currentTarget.style.boxShadow = '';
      }}
    >
      <Card.Section>
        <Image
          src={product.imageUrl}
          height={200}
          alt={product.name}
          fit="cover"
          style={{ cursor: 'pointer' }}
          onClick={() => navigate(`/app/products/${product.id}`)}
        />
      </Card.Section>

      <Stack mt="md" gap="xs">
        <Group justify="space-between" align="flex-start">
          <Text fw={500} size="lg" lineClamp={1} style={{ color: '#8B4513' }}>
            {product.name}
          </Text>
          <Badge 
            style={{
              background: 'linear-gradient(135deg, #D4A373 0%, #8B4513 100%)',
              border: 'none'
            }}
          >
            €{product.price}
          </Badge>
        </Group>

        <Text size="sm" c="dimmed" lineClamp={2}>
          {product.description}
        </Text>

        <Group gap="xs">
          <Badge 
            size="sm" 
            variant="dot" 
            color={product.stockQuantity > 0 ? "green" : "red"}
          >
            {product.stockQuantity > 0 ? "In Stock" : "Out of Stock"}
          </Badge>
          {product.reviewCount > 0 && (
            <Group gap={4}>
              <IconStar size={16} fill="#D4A373" color="#8B4513" />
              <Text size="sm" style={{ color: '#8B4513' }}>
                {product.averageRating.toFixed(1)} ({product.reviewCount})
              </Text>
            </Group>
          )}
        </Group>

        <Group mt="md" gap="xs">
          <Button
            variant="light"
            fullWidth
            onClick={() => navigate(`/app/products/${product.id}`)}
            color="brown"
          >
            View Details
          </Button>
          {onAddToCart && product.stockQuantity > 0 && (
            <Button
              leftSection={<IconShoppingCart size={18} />}
              onClick={() => onAddToCart(product.id)}
              style={{
                background: 'linear-gradient(135deg, #D4A373 0%, #8B4513 100%)',
                border: 'none'
              }}
            >
              Add
            </Button>
          )}
        </Group>
      </Stack>
    </Card>
  );
};

export default ProductCard;