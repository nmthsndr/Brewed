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
      await Promise.all([loadReviews(), checkUserReview(), loadProduct()]);
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
        await Promise.all([loadReviews(), checkUserReview(), loadProduct()]);
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
    <div>
      <Button
        leftSection={<IconArrowLeft size={16} />}
        variant="subtle"
        color="brown"
        onClick={() => navigate('/app/products')}
        mb="lg"
      >
        Back to Products
      </Button>

      <Grid gutter="xl">
        <Grid.Col span={{ base: 12, md: 6 }}>
          <Stack gap="sm">
            <div style={{
              width: '100%',
              height: '440px',
              overflow: 'hidden',
              borderRadius: '16px',
              backgroundColor: '#faf8f5',
              border: '1px solid rgba(139, 69, 19, 0.1)',
              boxShadow: '0 4px 20px rgba(139, 69, 19, 0.06)',
            }}>
              <img
                src={selectedImage || product.imageUrl}
                alt={product.name}
                style={{
                  width: '100%',
                  height: '100%',
                  objectFit: 'cover',
                  transition: 'transform 0.4s ease',
                }}
              />
            </div>
            {/* Thumbnail images */}
            {(product.productImages && product.productImages.length > 1) && (
              <Group gap="xs">
                {product.productImages.map((img) => (
                  <div
                    key={img.id}
                    style={{
                      width: '80px',
                      height: '80px',
                      overflow: 'hidden',
                      borderRadius: '10px',
                      cursor: 'pointer',
                      border: selectedImage === img.imageUrl ? '2px solid #8B4513' : '2px solid rgba(139, 69, 19, 0.15)',
                      transition: 'all 0.2s ease',
                      flexShrink: 0,
                      boxShadow: selectedImage === img.imageUrl ? '0 2px 8px rgba(139, 69, 19, 0.2)' : 'none',
                    }}
                    onClick={() => setSelectedImage(img.imageUrl)}
                  >
                    <img
                      src={img.imageUrl}
                      alt={`${product.name} - ${img.displayOrder}`}
                      style={{
                        width: '100%',
                        height: '100%',
                        objectFit: 'cover'
                      }}
                    />
                  </div>
                ))}
              </Group>
            )}
            {/* Fallback to imageUrls if no productImages */}
            {(!product.productImages || product.productImages.length === 0) && product.imageUrls && product.imageUrls.length > 1 && (
              <Group gap="xs">
                {product.imageUrls.map((url, index) => (
                  <div
                    key={index}
                    style={{
                      width: '80px',
                      height: '80px',
                      overflow: 'hidden',
                      borderRadius: '10px',
                      cursor: 'pointer',
                      border: selectedImage === url ? '2px solid #8B4513' : '2px solid rgba(139, 69, 19, 0.15)',
                      transition: 'all 0.2s ease',
                      flexShrink: 0,
                      boxShadow: selectedImage === url ? '0 2px 8px rgba(139, 69, 19, 0.2)' : 'none',
                    }}
                    onClick={() => setSelectedImage(url)}
                  >
                    <img
                      src={url}
                      alt={`${product.name} - ${index + 1}`}
                      style={{
                        width: '100%',
                        height: '100%',
                        objectFit: 'cover'
                      }}
                    />
                  </div>
                ))}
              </Group>
            )}
          </Stack>
        </Grid.Col>

        <Grid.Col span={{ base: 12, md: 6 }}>
          <Stack>
            <div>
              <Title order={2} style={{ color: '#3d3d3d' }}>{product.name}</Title>
              <Group mt="sm" gap="xs">
                <Badge
                  color={product.stockQuantity > 0 ? "green" : "red"}
                  variant="light"
                  radius="sm"
                >
                  {product.stockQuantity > 0 ? "In Stock" : "Out of Stock"}
                </Badge>
                <Badge variant="light" color="brown" radius="sm">{product.categoryName}</Badge>
              </Group>
            </div>

            {product.reviewCount > 0 && (
              <Group gap="xs">
                <Rating value={product.averageRating} readOnly fractions={2} color="brown" />
                <Text size="sm" c="dimmed">
                  {product.averageRating.toFixed(1)} ({product.reviewCount} reviews)
                </Text>
              </Group>
            )}

            <Text
              size="xl"
              fw={800}
              style={{
                color: '#8B4513',
                fontSize: '2rem',
              }}
            >
              €{product.price.toFixed(2)}
            </Text>

            <Text style={{ lineHeight: 1.7, color: '#5c5c5c' }}>{product.description}</Text>

            {/* Show coffee-specific fields only for Coffee Beans category */}
            {product.categoryName === "Coffee Beans" && (
              <Paper
                withBorder
                p="md"
                style={{
                  borderColor: 'rgba(139, 69, 19, 0.1)',
                  background: 'rgba(139, 69, 19, 0.02)',
                }}
              >
                <Stack gap="xs">
                  <Group>
                    <Text fw={600} size="sm" c="dimmed">Roast Level:</Text>
                    <Text size="sm">{product.roastLevel}</Text>
                  </Group>
                  <Group>
                    <Text fw={600} size="sm" c="dimmed">Origin:</Text>
                    <Text size="sm">{product.origin}</Text>
                  </Group>
                  <Group>
                    <Text fw={600} size="sm" c="dimmed">Caffeine Free:</Text>
                    <Text size="sm">{product.isCaffeineFree ? 'Yes' : 'No'}</Text>
                  </Group>
                  <Group>
                    <Text fw={600} size="sm" c="dimmed">Organic:</Text>
                    <Text size="sm">{product.isOrganic ? 'Yes' : 'No'}</Text>
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
                  style={{
                    background: 'linear-gradient(135deg, #D4A373 0%, #8B4513 100%)',
                    border: 'none',
                  }}
                >
                  Add to Cart
                </Button>
              </Group>
            )}
          </Stack>
        </Grid.Col>
      </Grid>

      <Divider my="xl" color="rgba(139, 69, 19, 0.1)" />

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