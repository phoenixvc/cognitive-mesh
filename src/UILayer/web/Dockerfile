# syntax=docker/dockerfile:1

###############################################################################
# Cognitive Mesh Frontend — Multi-stage Docker Build
#
# Build:   docker build -t cognitive-mesh-frontend .
# Run:     docker run -p 3000:3000 cognitive-mesh-frontend
###############################################################################

# ---------------------------------------------------------------------------
# Stage 1: Install dependencies
# ---------------------------------------------------------------------------
FROM node:22.22.2-alpine AS deps
WORKDIR /app

# Copy package manifests for layer caching
COPY package.json package-lock.json ./

# Install dependencies (--legacy-peer-deps required per project conventions)
RUN npm ci --legacy-peer-deps

# ---------------------------------------------------------------------------
# Stage 2: Build the Next.js application
# ---------------------------------------------------------------------------
FROM node:22.22.2-alpine AS builder
WORKDIR /app

# Build-time environment variables
ARG NEXT_PUBLIC_API_BASE_URL
ENV NEXT_PUBLIC_API_BASE_URL=${NEXT_PUBLIC_API_BASE_URL}

# Copy deps from previous stage
COPY --from=deps /app/node_modules ./node_modules

# Copy application source
COPY . .

# Build the application (standalone output configured in next.config.js)
RUN npm run build

# ---------------------------------------------------------------------------
# Stage 3: Production runtime
# ---------------------------------------------------------------------------
FROM node:22.22.2-alpine AS runner
WORKDIR /app

ENV NODE_ENV=production
ENV HOSTNAME=0.0.0.0
ENV PORT=3000

# Create non-root user for security
RUN addgroup --system --gid 1001 nodejs && \
    adduser --system --uid 1001 nextjs

# Copy public assets
COPY --from=builder /app/public ./public

# Set ownership on the .next directory for prerender cache
RUN mkdir .next && chown nextjs:nodejs .next

# Copy standalone server and static assets
COPY --from=builder --chown=nextjs:nodejs /app/.next/standalone ./
COPY --from=builder --chown=nextjs:nodejs /app/.next/static ./.next/static

# Switch to non-root user
USER nextjs

EXPOSE 3000

# Health check against the API route
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
  CMD wget --no-verbose --tries=1 --spider http://localhost:3000/api/health || exit 1

CMD ["node", "server.js"]
