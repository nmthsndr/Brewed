import { ActionIcon, Group, Image, Text } from "@mantine/core";
import {
  IconBrandInstagram,
  IconBrandTwitter,
  IconBrandFacebook,
} from "@tabler/icons-react";
import { useNavigate } from "react-router-dom";
import classes from "./Footer.module.css";

const links = [
  { label: "Products", url: "/app/products" },
  { label: "Cart", url: "/app/cart" },
  { label: "Dashboard", url: "/app/dashboard" },
];

const Footer = () => {
  const navigate = useNavigate();

  return (
    <div className={classes.inner}>
      <div className={classes.brand} onClick={() => navigate("/app/dashboard")}>
        <Image
          src="/logo.png"
          alt="Brewed Logo"
          h={32}
          w={32}
          style={{
            borderRadius: "50%",
            objectFit: "cover",
            border: "2px solid rgba(139, 69, 19, 0.12)",
          }}
        />
        <span className={classes.brandName}>Brewed</span>
      </div>

      <Group className={classes.links}>
        {links.map((link) => (
          <Text
            key={link.label}
            className={classes.link}
            onClick={() => navigate(link.url)}
          >
            {link.label}
          </Text>
        ))}
      </Group>

      <Group gap="xs" wrap="nowrap" className={classes.socials}>
        <ActionIcon size="lg" variant="subtle" color="brown" radius="xl" aria-label="Twitter">
          <IconBrandTwitter size={18} stroke={1.5} />
        </ActionIcon>
        <ActionIcon size="lg" variant="subtle" color="brown" radius="xl" aria-label="Facebook">
          <IconBrandFacebook size={18} stroke={1.5} />
        </ActionIcon>
        <ActionIcon size="lg" variant="subtle" color="brown" radius="xl" aria-label="Instagram">
          <IconBrandInstagram size={18} stroke={1.5} />
        </ActionIcon>
      </Group>
    </div>
  );
};

export default Footer;