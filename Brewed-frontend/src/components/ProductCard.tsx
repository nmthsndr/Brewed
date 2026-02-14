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
      radius="lg"
      withBorder
      style={{
        borderColor: 'rgba(139, 69, 19, 0.1)',
        transition: 'all 0.3s cubic-bezier(0.4, 0, 0.2, 1)',
        cursor: 'pointer',
        overflow: 'hidden',
      }}
      onMouseEnter={(e) => {
        e.currentTarget.style.transform = 'translateY(-6px)';
        e.currentTarget.style.boxShadow = '0 12px 32px rgba(139, 69, 19, 0.12)';
        e.currentTarget.style.borderColor = 'rgba(139, 69, 19, 0.2)';
      }}
      onMouseLeave={(e) => {
        e.currentTarget.style.transform = 'translateY(0)';
        e.currentTarget.style.boxShadow = '';
        e.currentTarget.style.borderColor = 'rgba(139, 69, 19, 0.1)';
      }}
    >
      <Card.Section style={{ position: 'relative', overflow: 'hidden' }}>
        <Image
          src={product.imageUrl}
          height={220}
          alt={product.name}
          fit="cover"
          style={{
            cursor: 'pointer',
            transition: 'transform 0.4s cubic-bezier(0.4, 0, 0.2, 1)',
          }}
          onClick={() => navigate(`/app/products/${product.id}`)}
          onMouseEnter={(e) => { e.currentTarget.style.transform = 'scale(1.05)'; }}
          onMouseLeave={(e) => { e.currentTarget.style.transform = 'scale(1)'; }}
        />
        <Badge
          size="lg"
          style={{
            position: 'absolute',
            top: 12,
            right: 12,
            background: 'linear-gradient(135deg, #D4A373 0%, #8B4513 100%)',
            border: 'none',
            boxShadow: '0 2px 8px rgba(139, 69, 19, 0.3)',
            fontSize: '0.85rem',
            fontWeight: 700,
          }}
        >
          €{product.price}
        </Badge>
      </Card.Section>

      <Stack mt="md" gap="sm">
        <Text fw={600} size="lg" lineClamp={1} style={{ color: '#3d3d3d' }}>
          {product.name}
        </Text>

        <Text size="sm" c="dimmed" lineClamp={2} style={{ lineHeight: 1.5 }}>
          {product.description}
        </Text>

        <Group gap="xs">
          <Badge
            size="sm"
            variant="light"
            color={product.stockQuantity > 0 ? "green" : "red"}
            radius="sm"
          >
            {product.stockQuantity > 0 ? "In Stock" : "Out of Stock"}
          </Badge>
          {product.reviewCount > 0 && (
            <Group gap={4}>
              <IconStar size={14} fill="#D4A373" color="#D4A373" />
              <Text size="sm" fw={500} style={{ color: '#8B4513' }}>
                {product.averageRating.toFixed(1)}
              </Text>
              <Text size="xs" c="dimmed">
                ({product.reviewCount})
              </Text>
            </Group>
          )}
        </Group>

        <Group mt="xs" gap="xs">
          <Button
            variant="light"
            fullWidth
            onClick={() => navigate(`/app/products/${product.id}`)}
            color="brown"
            radius="md"
          >
            View Details
          </Button>
          {onAddToCart && product.stockQuantity > 0 && (
            <Button
              leftSection={<IconShoppingCart size={16} />}
              onClick={() => onAddToCart(product.id)}
              radius="md"
              style={{
                background: 'linear-gradient(135deg, #D4A373 0%, #8B4513 100%)',
                border: 'none',
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