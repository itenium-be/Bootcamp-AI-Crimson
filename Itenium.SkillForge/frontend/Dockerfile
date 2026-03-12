FROM oven/bun:1

WORKDIR /app

ARG NPM_TOKEN

# Configure npm auth for GitHub Packages
RUN echo "//npm.pkg.github.com/:_authToken=${NPM_TOKEN}" >> ~/.npmrc && \
    echo "@itenium-forge:registry=https://npm.pkg.github.com" >> ~/.npmrc

COPY package.json bun.lock ./
RUN bun install --frozen-lockfile

COPY . .

EXPOSE 5173

CMD ["bun", "run", "dev", "--host", "0.0.0.0"]
