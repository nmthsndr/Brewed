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
    <Card shadow="sm" padding="lg" radius="md" withBorder>
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
          <Text fw={500} size="lg" lineClamp={1}>
            {product.name}
          </Text>
          <Badge color="blue" variant="light">
            ${product.price}
          </Badge>
        </Group>

        <Text size="sm" c="dimmed" lineClamp={2}>
          {product.description}
        </Text>

        <Group gap="xs">
          <Badge size="sm" variant="dot" color={product.stockQuantity > 0 ? "green" : "red"}>
            {product.stockQuantity > 0 ? "In Stock" : "Out of Stock"}
          </Badge>
          {product.reviewCount > 0 && (
            <Group gap={4}>
              <IconStar size={16} fill="gold" color="gold" />
              <Text size="sm">
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
          >
            View Details
          </Button>
          {onAddToCart && product.stockQuantity > 0 && (
            <Button
              leftSection={<IconShoppingCart size={18} />}
              onClick={() => onAddToCart(product.id)}
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