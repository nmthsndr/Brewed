import { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import {
  Title,
  Text,
  Image,
  Group,
  Badge,
  Button,
  Stack,
  Grid,
  Paper,
  LoadingOverlay,
  NumberInput,
  Divider,
  Card,
  Rating,
  Textarea,
  ActionIcon,
  TextInput
} from "@mantine/core";
import { IconShoppingCart, IconArrowLeft, IconStar } from "@tabler/icons-react";
import { useForm } from "@mantine/form";
import api, { ReviewCreateDto } from "../api/api";
import { IProduct } from "../interfaces/IProduct";
import { IReview } from "../interfaces/IReview";
import ReviewCard from "../components/ReviewCard";
import { notifications } from "@mantine/notifications";
import useAuth from "../hooks/useAuth";
import useCart from "../hooks/useCart";
import { getGuestSessionId } from "../utils/guestSession";

const ProductDetail = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const { userId, isLoggedIn } = useAuth();
  const { refreshCartCount } = useCart();
  const [product, setProduct] = useState<IProduct | null>(null);
  const [reviews, setReviews] = useState<IReview[]>([]);
  const [loading, setLoading] = useState(true);
  const [quantity, setQuantity] = useState(1);
  const [hasPurchased, setHasPurchased] = useState(false);
  const [hasReviewed, setHasReviewed] = useState(false);
  const [userReview, setUserReview] = useState<IReview | null>(null);
  const [selectedImage, setSelectedImage] = useState<string>('');

  const reviewForm = useForm<ReviewCreateDto>({
    initialValues: {
      productId: 0,
      rating: 5,
      title: '',
      comment: ''
    },
    validate: {
      rating: (val) => (val < 1 || val > 5 ? 'Rating must be between 1 and 5' : null),
      comment: (val) => (val.length < 10 ? 'Comment must be at least 10 characters' : null)
    }
  });

  const loadProduct = async () => {
    try {
      setLoading(true);
      //console.log("Loading product with ID:", id);

      if (!id || isNaN(parseInt(id))) {
        throw new Error("Invalid product ID");
      }

      const response = await api.Products.getProduct(parseInt(id));
      //console.log("Product loaded successfully:", response.data);
      setProduct(response.data);

      // Set initial selected image - prefer productImages first, then imageUrls, then imageUrl
      if (response.data.productImages && response.data.productImages.length > 0) {
        setSelectedImage(response.data.productImages[0].imageUrl);
      } else if (response.data.imageUrls && response.data.imageUrls.length > 0) {
        setSelectedImage(response.data.imageUrls[0]);
      } else {
        setSelectedImage(response.data.imageUrl);
      }

      reviewForm.setFieldValue('productId', response.data.id);
    } catch (error) {
      console.error("Failed to load product:", error);
      notifications.show({
        title: 'Error',
        message: 'Failed to load product details',
        color: 'red',
      });
      // Navigate back to products page on error
      navigate('/app/products');
    } finally {
      setLoading(false);
    }
  };

  const loadReviews = async () => {
    try {
      const response = await api.Reviews.getProductReviews(parseInt(id!), 1, 10);
      setReviews(response.data.items);
    } catch (error) {
      console.error("Failed to load reviews:", error);
    }
  };

  const checkPurchaseStatus = async () => {
    if (!isLoggedIn || !id) {
      setHasPurchased(false);
      return;
    }

    try {
      const response = await api.Products.hasPurchasedProduct(parseInt(id));
      setHasPurchased(response.data.hasPurchased);
    } catch (error) {
      console.error("Failed to check purchase status:", error);
      setHasPurchased(false);
    }
  };

  const checkUserReview = async () => {
    if (!isLoggedIn || !id) {
      setHasReviewed(false);
      setUserReview(null);
      return;
    }

    try {
      const response = await api.Reviews.getUserReviewForProduct(parseInt(id));
      setHasReviewed(response.data.hasReviewed);
      setUserReview(response.data.review);
    } catch (error) {
      console.error("Failed to check user review:", error);
      setHasReviewed(false);
      setUserReview(null);
    }
  };

  useEffect(() => {
    if (id) {
      loadProduct();
      loadReviews();
      checkPurchaseStatus();
      checkUserReview();
    }
  }, [id, isLoggedIn]);

  const handleAddToCart = async () => {
    if (!product) return;

    try {
      const sessionId = isLoggedIn ? undefined : getGuestSessionId();
      await api.Cart.addToCart({ productId: product.id, quantity }, sessionId);
      await refreshCartCount();
      notifications.show({
        title: 'Success',
        message: `${quantity} ${product.name} added to cart`,
        color: 'green',
      });
    } catch (error: any) {
      notifications.show({
        title: 'Error',
        message: error.response?.data || 'Failed to add product to cart',
        color: 'red',
      });
    }
  };

  const handleSubmitReview = async (values: ReviewCreateDto) => {
    try {
      await api.Reviews.createReview(values);
      notifications.show({
        title: 'Success',
        message: 'Review submitted successfully',
        color: 'green',
      });
      reviewForm.reset();
      await loadReviews();
      await checkUserReview();
    } catch (error: any) {
      notifications.show({
        title: 'Error',
        message: error.response?.data || 'Failed to submit review',
        color: 'red',
      });
    }
  };

  const handleDeleteReview = async (reviewId: number) => {
    if (window.confirm('Are you sure you want to delete this review?')) {
      try {
        await api.Reviews.deleteReview(reviewId);
        notifications.show({
          title: 'Success',
          message: 'Review deleted successfully',
          color: 'green',
        });
        await loadReviews();
        await checkUserReview();
      } catch (error) {
        notifications.show({
          title: 'Error',
          message: 'Failed to delete review',
          color: 'red',
        });
      }
    }
  };

  if (loading || !product) {
    return <LoadingOverlay visible />;
  }

  return (
    <div style={{ padding: '20px' }}>
      <Button
        leftSection={<IconArrowLeft size={16} />}
        variant="subtle"
        onClick={() => navigate('/app/products')}
        mb="lg"
      >
        Back to Products
      </Button>

      <Grid>
        <Grid.Col span={{ base: 12, md: 6 }}>
          <Stack gap="sm">
            <Image
              src={selectedImage || product.imageUrl}
              alt={product.name}
              radius="md"
              height={400}
              fit="cover"
            />
            {/* Thumbnail images */}
            {(product.productImages && product.productImages.length > 1) && (
              <Group gap="xs">
                {product.productImages.map((img) => (
                  <Image
                    key={img.id}
                    src={img.imageUrl}
                    alt={`${product.name} - ${img.displayOrder}`}
                    radius="sm"
                    height={80}
                    width={80}
                    fit="cover"
                    style={{
                      cursor: 'pointer',
                      border: selectedImage === img.imageUrl ? '2px solid #228be6' : '2px solid transparent',
                      transition: 'border 0.2s'
                    }}
                    onClick={() => setSelectedImage(img.imageUrl)}
                  />
                ))}
              </Group>
            )}
            {/* Fallback to imageUrls if no productImages */}
            {(!product.productImages || product.productImages.length === 0) && product.imageUrls && product.imageUrls.length > 1 && (
              <Group gap="xs">
                {product.imageUrls.map((url, index) => (
                  <Image
                    key={index}
                    src={url}
                    alt={`${product.name} - ${index + 1}`}
                    radius="sm"
                    height={80}
                    width={80}
                    fit="cover"
                    style={{
                      cursor: 'pointer',
                      border: selectedImage === url ? '2px solid #228be6' : '2px solid transparent',
                      transition: 'border 0.2s'
                    }}
                    onClick={() => setSelectedImage(url)}
                  />
                ))}
              </Group>
            )}
          </Stack>
        </Grid.Col>

        <Grid.Col span={{ base: 12, md: 6 }}>
          <Stack>
            <div>
              <Title order={2}>{product.name}</Title>
              <Group mt="xs" gap="xs">
                <Badge color={product.stockQuantity > 0 ? "green" : "red"}>
                  {product.stockQuantity > 0 ? "In Stock" : "Out of Stock"}
                </Badge>
                <Badge variant="light">{product.categoryName}</Badge>
              </Group>
            </div>

            {product.reviewCount > 0 && (
              <Group gap="xs">
                <Rating value={product.averageRating} readOnly fractions={2} />
                <Text size="sm">
                  {product.averageRating.toFixed(1)} ({product.reviewCount} reviews)
                </Text>
              </Group>
            )}

            <Text size="xl" fw={700} c="blue">
            €{product.price.toFixed(2)}
            </Text>

            <Text>{product.description}</Text>

            {/* Show coffee-specific fields only for Coffee Beans category */}
            {product.categoryName === "Coffee Beans" && (
              <Paper withBorder p="md">
                <Stack gap="xs">
                  <Group>
                    <Text fw={500}>Roast Level:</Text>
                    <Text>{product.roastLevel}</Text>
                  </Group>
                  <Group>
                    <Text fw={500}>Origin:</Text>
                    <Text>{product.origin}</Text>
                  </Group>
                  <Group>
                    <Text fw={500}>Caffeine Free:</Text>
                    <Text>{product.isCaffeineFree ? 'Yes' : 'No'}</Text>
                  </Group>
                  <Group>
                    <Text fw={500}>Organic:</Text>
                    <Text>{product.isOrganic ? 'Yes' : 'No'}</Text>
                  </Group>
                </Stack>
              </Paper>
            )}

            {product.stockQuantity > 0 && (
              <Group>
                <NumberInput
                  label="Quantity"
                  value={quantity}
                  onChange={(val) => setQuantity(Number(val))}
                  min={1}
                  max={product.stockQuantity}
                  style={{ width: 100 }}
                />
                <Button
                  leftSection={<IconShoppingCart size={18} />}
                  onClick={handleAddToCart}
                  mt="xl"
                  size="lg"
                >
                  Add to Cart
                </Button>
              </Group>
            )}
          </Stack>
        </Grid.Col>
      </Grid>

      <Divider my="xl" />

      {/* Reviews Section */}
      <Title order={3} mb="md">Customer Reviews</Title>

      <Grid>
        <Grid.Col span={{ base: 12, md: 6 }}>
          <Card withBorder p="lg">
            <Title order={4} mb="md">Write a Review</Title>
            {!isLoggedIn ? (
              <Text c="dimmed" ta="center">
                Please log in to write a review
              </Text>
            ) : !hasPurchased ? (
              <Text c="dimmed" ta="center">
                You can only review products you have purchased and received
              </Text>
            ) : hasReviewed && userReview ? (
              <Stack>
                <Text c="dimmed" ta="center" mb="md">
                  You have already reviewed this product
                </Text>
                <ReviewCard
                  review={userReview}
                  canDelete={true}
                  onDelete={handleDeleteReview}
                />
              </Stack>
            ) : (
              <form onSubmit={reviewForm.onSubmit(handleSubmitReview)}>
                <Stack>
                  <div>
                    <Text size="sm" fw={500} mb="xs">Rating</Text>
                    <Rating {...reviewForm.getInputProps('rating')} size="lg" />
                  </div>

                  <TextInput
                    label="Title (Optional)"
                    placeholder="Summary of your review"
                    {...reviewForm.getInputProps('title')}
                  />

                  <Textarea
                    label="Review"
                    placeholder="Share your thoughts about this product"
                    required
                    minRows={4}
                    {...reviewForm.getInputProps('comment')}
                  />

                  <Button type="submit">Submit Review</Button>
                </Stack>
              </form>
            )}
          </Card>
        </Grid.Col>

        <Grid.Col span={{ base: 12, md: 6 }}>
          <Stack>
            {reviews.length === 0 ? (
              <Text c="dimmed" ta="center">No reviews yet. Be the first to review!</Text>
            ) : (
              reviews.map((review) => (
                <ReviewCard
                  key={review.id}
                  review={review}
                  canDelete={review.userId.toString() === userId}
                  onDelete={handleDeleteReview}
                />
              ))
            )}
          </Stack>
        </Grid.Col>
      </Grid>
    </div>
  );
};

export default ProductDetail;